"use strict";

const $ = selector => document.querySelector(selector);
const state = {
  token: sessionStorage.getItem("sportsBuddyToken"),
  user: null, sports: [], playerSkills: [], locations: [], events: [], registrations: [], connections: [], feed: [], activeEvent: null, ready: false
};
const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

function element(tag, text, className) {
  const node = document.createElement(tag);
  if (text !== undefined) node.textContent = text;
  if (className) node.className = className;
  return node;
}

function notice(message = "", error = false) {
  $("#status").textContent = message;
  $("#status").hidden = !message;
  $("#status").classList.toggle("error", error);
}

function signOut() {
  notice();
  sessionStorage.removeItem("sportsBuddyToken");
  Object.assign(state, { token: null, user: null, ready: false, sports: [], playerSkills: [], locations: [], events: [], registrations: [], connections: [], feed: [], activeEvent: null });
  $("#login-form").reset();
  location.hash = "#login";
  showPage();
}

async function api(path, method = "GET", body) {
  const headers = {};
  if (state.token) headers.Authorization = "Bearer " + state.token;
  if (body !== undefined && !(body instanceof FormData)) headers["Content-Type"] = "application/json";
  const response = await fetch("/api/" + path, {
    method, headers, body: body === undefined ? undefined : body instanceof FormData ? body : JSON.stringify(body)
  });
  if (response.status === 204) return null;
  const type = response.headers.get("content-type") || "";
  const data = type.includes("json") ? await response.json() : await response.text();
  if (!response.ok) {
    if (response.status === 401 && !path.startsWith("Auth/")) signOut();
    const message = response.status === 401
      ? (path.startsWith("Auth/") ? "Email or password is incorrect." : "Your session has expired. Please log in again.")
      : (typeof data === "string" && data.length < 250 ? data : data.errors
        ? Object.values(data.errors).flat().join(" ") : data.title || "The request could not be completed.");
    const error = new Error(response.status >= 500 ? "The server could not complete the request. Please try again." : message);
    error.status = response.status;
    throw error;
  }
  return data;
}

async function submit(form, work) {
  const button = form.querySelector('button[type="submit"]');
  const label = button.textContent;
  button.disabled = true;
  button.textContent = "Working…";
  notice();
  try { await work(new FormData(form)); }
  catch (error) { notice(error.message, true); }
  finally {
    button.disabled = false;
    if (button.textContent === "Working…") button.textContent = label;
  }
}

function rangeValue(value) { return value === "" ? null : Number(value); }
function sportName(id) { return state.sports.find(s => s.id === id)?.name || "Sport unavailable"; }
function addressText(address) {
  return address ? [address.street, address.postcode, address.city, address.country].filter(Boolean).join(", ") : "Address unavailable";
}
function locationText(id) { return addressText(state.locations.find(l => l.id === id)); }

function options(select, items, value, label, placeholder = "Choose a sport") {
  const selected = select.value;
  select.replaceChildren(new Option(placeholder, ""));
  for (const item of items) select.add(new Option(label(item), String(value(item))));
  if (items.some(item => String(value(item)) === selected)) select.value = selected;
}

async function loadData() {
  const [user, sports, playerSkills, events, registrations, connections] = await Promise.all([
    api("Users/me"), api("Skills"), api("PlayerSkills"), api("Events/joined"), api("UserEvents"), api("Connections")
  ]);
  // Fetch addresses in use; an unavailable legacy address should not hide the whole profile.
  const locationIds = [...new Set([user.location, ...events.map(e => e.location)])];
  const locations = await Promise.all(locationIds.map(id => api("Locations/" + id).catch(error => {
    if (error.status === 401) throw error;
    return null;
  })));
  Object.assign(state, { user, sports, playerSkills, events, registrations, connections, locations: locations.filter(Boolean), ready: true });
  renderProfile();
  renderSports();
  renderEvents();
  renderConnections();
}

function renderProfile() {
  const details = $("#profile-details");
  details.replaceChildren();
  for (const [title, value] of [["Name", state.user.name], ["Email", state.user.email], ["Home address", locationText(state.user.location)]]) {
    details.append(element("dt", title), element("dd", value));
  }
  $("#preferred-range").value = state.user.preferredRangeKm ?? "";
  $("#profile-form").elements.name.value = state.user.name;
  $("#profile-form").elements.bio.value = state.user.bio || "";
  $("#my-avatar").replaceChildren(avatar(state.user.name, state.user.profilePictureUrl));
  $("#my-profile-name").textContent = state.user.name;
  const home = state.locations.find(l => l.id === state.user.location);
  $("#my-profile-city").textContent = home ? home.city + ", " + home.country : "";
  const ranges = [1, 2, 5, 10, 15, 25, 50, 100, 250];
  if (state.user.preferredRangeKm !== null && !ranges.includes(state.user.preferredRangeKm)) ranges.push(state.user.preferredRangeKm);
  $("#header-range").replaceChildren(new Option("Set distance", ""));
  ranges.sort((a,b) => a-b).forEach(r => $("#header-range").add(new Option(r + " km", String(r))));
  $("#header-range").value = state.user.preferredRangeKm ?? "";
  $("#availability-rows").replaceChildren();
  state.user.availabilities.forEach(addSlot);
}

function renderSports() {
  options($("#skill-choice"), state.sports, s => s.id, s => s.name);
  const mySports = state.playerSkills.map(p => ({ id: p.skillId, name: sportName(p.skillId) }));
  options($("#feed-sport"), mySports, s => s.id, s => s.name);
  if (!$("#feed-sport").value && mySports.length) $("#feed-sport").value = mySports[0].id;
  const list = $("#my-skills");
  list.replaceChildren();
  if (!state.playerSkills.length) list.append(element("p", "You have not added any sports yet.", "hint"));
  for (const player of state.playerSkills) {
    const card = element("article", undefined, "small-card");
    card.append(element("h3", sportName(player.skillId)), element("p", "Skill level: " + player.skillLevel + " / 5"));
    const actions = element("div", undefined, "actions");
    const edit = element("button", "Edit", "secondary");
    edit.type = "button";
    edit.addEventListener("click", () => editSkill(player));
    const remove = element("button", "Remove", "danger");
    remove.type = "button";
    remove.addEventListener("click", async () => {
      remove.disabled = true;
      try {
        await api("PlayerSkills/" + player.id, "DELETE");
        await loadData();
        resetSkillForm();
        notice("Sport removed.");
      } catch (error) { notice(error.message, true); remove.disabled = false; }
    });
    actions.append(edit, remove);
    card.append(actions);
    list.append(card);
  }
}

function addSlot(slot = { dayOfWeek: 2, startTime: "18:00", endTime: "20:00" }) {
  const row = element("div", undefined, "slot");
  const dayLabel = element("label", "Day");
  const day = element("select");
  day.className = "slot-day";
  day.required = true;
  days.forEach((name, index) => day.add(new Option(name, String(index))));
  day.value = slot.dayOfWeek;
  dayLabel.append(day);
  const times = element("div", undefined, "slot-times");
  for (const [title, name, value] of [["From", "slot-start", slot.startTime], ["Until", "slot-end", slot.endTime]]) {
    const label = element("label", title);
    const input = element("input");
    input.type = "time"; input.className = name; input.required = true; input.value = value.slice(0, 5);
    label.append(input);
    times.append(label);
  }
  const remove = element("button", "Remove time slot", "secondary");
  remove.type = "button";
  remove.addEventListener("click", () => row.remove());
  row.append(dayLabel, times, remove);
  $("#availability-rows").append(row);
}

function resetSkillForm() {
  $("#skill-form").reset();
  $("#skill-form").elements.playerSkillId.value = "";
  $("#skill-form-title").textContent = "Add a sport";
  $("#save-skill").textContent = "Add sport";
  $("#cancel-skill").hidden = true;
}

function editSkill(player) {
  $("#skill-form").elements.playerSkillId.value = player.id;
  $("#skill-choice").value = player.skillId;
  $("#skill-form").elements.skillLevel.value = player.skillLevel;
  $("#skill-form-title").textContent = "Edit sport";
  $("#save-skill").textContent = "Save sport";
  $("#cancel-skill").hidden = false;
  $("#skill-form").scrollIntoView({ behavior: "smooth", block: "center" });
}

async function loadFeed() {
  const skillId = $("#feed-sport").value;
  $("#match-list").replaceChildren(); $("#people-list").replaceChildren();
  $("#feed-note").textContent = "";
  if (!skillId) { $("#feed-note").textContent = "Add a sport on My profile to find your next game."; return; }
  if (state.user.preferredRangeKm === null) { $("#feed-note").textContent = "Set a travel distance above or on My profile to find local matches."; return; }
  $("#feed-note").textContent = "Checking the noticeboard…";
  const feed = await api("Matches/feed?skillId=" + encodeURIComponent(skillId));
  if ($("#feed-sport").value !== skillId) return;
  state.feed = feed;
  renderFeed();
}

function avatar(name, url, className = "") {
  if (url) { const img = element("img"); img.src = url; img.alt = name; img.className = "avatar " + className; return img; }
  const initials = name.replace(/\(Demo\)/g, "").trim().split(/\s+/).slice(0,2).map(n => n[0]).join("");
  return element("div", initials, "avatar avatar-initials " + className);
}
function link(text, href, className = "") { const a = element("a", text, className); a.href = href; return a; }
function defaultPhoto(skillId) { return /running|cycling|walking|hiking|triathlon|athletics/i.test(sportName(skillId)) ? "/images/running.jpg" : "/images/basketball.jpg"; }
function levelName(level) { return ["", "Beginner", "Basic", "Intermediate", "Advanced", "Expert"][level] || "All levels"; }
function dateText(date) { return new Date(date).toLocaleString("en-GB", {dateStyle:"medium",timeStyle:"short"}); }
function sessionTime(date, endDate) {
  const start = new Date(date);
  const day = start.toLocaleDateString("en-GB", {weekday:"short",day:"numeric",month:"short"});
  const time = start.toLocaleTimeString("en-GB", {hour:"2-digit",minute:"2-digit"});
  return day + " · " + time + (endDate ? " – " + new Date(endDate).toLocaleTimeString("en-GB",{hour:"2-digit",minute:"2-digit"}) : "");
}
function places(item) { return item.participantCount + " / " + (item.maxParticipants ?? "no limit") + " joined"; }
async function action(button, work) {
  button.disabled = true; notice();
  try { await work(); } catch(error) { notice(error.message, true); }
  finally { button.disabled = false; }
}
function membershipButton(item, registration) {
  const full = item.maxParticipants !== null && item.participantCount >= item.maxParticipants;
  const button = element("button", registration ? "Leave event" : full ? "Full" : "Join event →", registration ? "secondary" : "");
  button.type = "button"; button.disabled = !registration && full;
  button.addEventListener("click", () => action(button, async () => {
    if (registration) await api("UserEvents/" + registration.id, "DELETE");
    else await api("UserEvents", "POST", {eventId:item.id});
    await loadData();
    if (location.hash === "#feed") await loadFeed();
    else if (location.hash.startsWith("#event/")) await loadEvent(Number(location.hash.split("/")[1]));
    notice(registration ? "You left the session." : "You're in. Find this session in your Events tab.");
  }));
  return button;
}
function eventCard(item, skillId, joined = false) {
  const card = element("article", undefined, "session-card");
  const image = element("img"); image.className = "session-image"; image.src = item.pictureUrl || defaultPhoto(skillId); image.alt = item.name;
  const body = element("div", undefined, "session-body");
  body.append(element("p", sessionTime(item.date, item.endDate), "kicker"));
  const title = element("h3"); title.append(link(item.name, "#event/" + item.id));
  body.append(title);
  const meta = element("div", undefined, "session-meta");
  meta.append(element("span", "⌖ " + (item.city || locationText(item.location))), element("span", item.distanceKm === undefined ? "" : item.distanceKm + " km away"));
  body.append(meta, element("p", item.bio || "A local " + sportName(skillId).toLowerCase() + " session. Open the event for details.", "bio-excerpt"));
  const facts = element("div", undefined, "session-facts");
  facts.append(element("span", "▥ " + levelName(item.skillLevel)), element("span", "♟ " + places(item)));
  body.append(facts);
  const bottom = element("div", undefined, "session-bottom");
  bottom.append(item.score === undefined ? element("span", sportName(skillId), "hint") : element("span", item.score.toFixed(2) + "% match", "score"));
  const buttons = element("div", undefined, "actions");
  buttons.append(link("View event →", "#event/" + item.id, "button secondary"), membershipButton(item, joined ? state.registrations.find(r => r.eventId === item.id) : null));
  bottom.append(buttons); body.append(bottom); card.append(image, body); return card;
}
function connectionButton(userId) {
  const connection = state.connections.find(c => c.otherUserId === userId);
  const label = !connection ? "Connect +" : connection.status === "connected" ? "Connected" : connection.isIncoming ? "Accept connection" : "Request sent";
  const button = element("button", label, "secondary"); button.type = "button";
  button.disabled = !!connection && (connection.status === "connected" || !connection.isIncoming);
  button.addEventListener("click", () => action(button, async () => {
    if (connection?.isIncoming) await api("Connections/" + connection.id + "/accept", "POST");
    else await api("Connections", "POST", {targetUserId:userId});
    await refreshConnections();
    notice(connection?.isIncoming ? "You're connected." : "Connection request sent.");
  }));
  return button;
}
async function refreshConnections() {
  state.connections = await api("Connections"); renderConnections();
  if (location.hash === "#feed") renderFeed();
  else if (location.hash.startsWith("#member/")) await loadMember(Number(location.hash.split("/")[1]));
}
function personCard(item) {
  const card = element("article", undefined, "person-card"); card.append(avatar(item.name, item.pictureUrl));
  const body = element("div"); const h = element("h3"); h.append(link(item.name, "#member/" + item.id));
  body.append(h, element("p", sportName(Number($("#feed-sport").value)) + " · " + levelName(item.skillLevel) + " · " + item.distanceKm + " km away", "meta"),
    element("p", item.bio ? "“" + item.bio + "”" : "A local player looking for good games and good company.", "bio-excerpt"));
  const actions = element("div", undefined, "person-actions"); actions.append(connectionButton(item.id), link("View profile →", "#member/" + item.id));
  body.append(actions); card.append(body); return card;
}
function renderFeed() {
  const events = state.feed.filter(m => m.type === "event");
  const people = state.feed.filter(m => m.type === "user");
  if ($("#feed-sort").value === "date") events.sort((a,b) => new Date(a.date) - new Date(b.date));
  $("#feed-note").textContent = events.length + " sessions · " + people.length + " potential buddies";
  $("#match-list").replaceChildren(...events.map(e => eventCard(e, Number($("#feed-sport").value))));
  $("#people-list").replaceChildren(...people.map(personCard));
  if (!events.length) $("#match-list").append(element("p", "No matching sessions yet. Events need to fit your sport, travel distance and availability. Why not organise one?", "empty"));
  if (!people.length) $("#people-list").append(element("p", "No matching players yet. Check your sports and travel distance on your profile.", "empty"));
}

function eventAddressMode() {
  const isNew = $("#event-location").value === "new";
  $("#event-address").hidden = !isNew;
  $("#event-address").querySelectorAll("input").forEach(input => { input.required = isNew; });
}

function renderEvents() {
  options($("#event-sport"), state.sports, s => s.id, s => s.name);
  const select = $("#event-location");
  options(select, state.locations, l => l.id, addressText, "Choose a location");
  select.add(new Option("Use a new address", "new"));
  if (!select.value) select.value = String(state.user.location);
  eventAddressMode();
  $("#event-list").replaceChildren(...[...state.events].sort((a,b) => new Date(a.date)-new Date(b.date)).map(e => eventCard(e, e.skillId, true)));
  if (!state.events.length) {
    const empty = element("p", "No plans yet. ", "empty"); empty.append(link("Find a session on the noticeboard →", "#feed")); $("#event-list").append(empty);
  }
}
function renderConnections() {
  const list = $("#connection-list"); list.replaceChildren();
  if (!state.connections.length) { list.append(element("p", "Your sporting circle starts with a connection. Meet players on the noticeboard.", "empty")); return; }
  for (const c of state.connections) {
    const card = element("article", undefined, "person-card"); card.append(avatar(c.name, c.profilePictureUrl));
    const body = element("div"); const title = element("h3"); title.append(link(c.name, "#member/" + c.otherUserId));
    body.append(title, element("p", c.city + " · " + (c.status === "connected" ? "Connected" : c.isIncoming ? "Incoming request" : "Request sent"), "meta"));
    const actions = element("div", undefined, "person-actions");
    if (c.isIncoming) actions.append(connectionButton(c.otherUserId));
    const remove = element("button", c.status === "connected" ? "Disconnect" : c.isIncoming ? "Decline" : "Cancel request", "secondary");
    remove.type = "button"; remove.addEventListener("click", () => action(remove, async () => { await api("Connections/" + c.id, "DELETE"); await refreshConnections(); notice("Connection updated."); }));
    actions.append(remove); body.append(actions); card.append(body); list.append(card);
  }
}
async function loadMember(id) {
  const container = $("#member-profile"); container.replaceChildren();
  const profile = await api("Users/" + id);
  const portrait = avatar(profile.name, profile.profilePictureUrl, "portrait");
  const content = element("div"); content.append(element("span", "MEET YOUR NEXT SPORT BUDDY", "eyebrow"), element("h1", profile.name),
    element("p", "⌖ " + profile.city + ", " + profile.country), element("p", profile.bio || "This player hasn't added a bio yet.", "full-bio"));
  const sports = element("div", undefined, "sport-chips"); profile.sports.forEach(sport => sports.append(element("span", sport.name + " · " + levelName(sport.skillLevel), "sport-chip")));
  content.append(sports, id === state.user.id ? link("Edit my profile →", "#profile", "button") : connectionButton(id));
  container.append(portrait, content);
}
async function loadEvent(id) {
  const item = await api("Events/" + id); state.activeEvent = item;
  const address = await api("Locations/" + item.location);
  const container = $("#event-detail"); container.replaceChildren();
  const layout = element("div", undefined, "event-detail-layout");
  const image = element("img"); image.src = item.pictureUrl || defaultPhoto(item.skillId); image.alt = item.name; image.className = "event-hero";
  const content = element("div"); content.append(element("span", sportName(item.skillId).toUpperCase(), "eyebrow"), element("h1", item.name),
    element("p", dateText(item.date) + (item.endDate ? " – " + dateText(item.endDate) : "")), element("p", "⌖ " + addressText(address)),
    element("p", levelName(item.skillLevel) + " · " + places(item)), element("p", item.bio || "The organiser hasn't added a description yet.", "full-bio"),
    membershipButton(item, state.registrations.find(r => r.eventId === item.id)));
  layout.append(image, content); container.append(layout);
  const owner = item.createdById === state.user.id; $("#event-owner-panel").hidden = !owner;
  if (owner) {
    options($("#edit-event-sport"), state.sports, s => s.id, s => s.name);
    const f = $("#event-edit-form"); f.elements.name.value = item.name; f.elements.bio.value = item.bio || "";
    f.elements.skillId.value = item.skillId ?? ""; f.elements.skillLevel.value = item.skillLevel ?? "3";
    f.elements.maxParticipants.value = item.maxParticipants ?? ""; f.elements.date.value = item.date.slice(0,16);
    f.elements.endDate.value = item.endDate?.slice(0,16) || "";
  }
}

async function showPage() {
  const route = location.hash.slice(1) || (state.token ? "feed" : "login");
  let page = route.split("/")[0];
  if (!["login","register","feed","profile","events","member","event"].includes(page)) page = state.token ? "feed" : "login";
  if (!state.token && !["login","register"].includes(page)) { location.hash = "#login"; return; }
  if (state.token && ["login","register"].includes(page)) { location.hash = "#feed"; return; }
  document.querySelectorAll("[data-page]").forEach(section => { section.hidden = section.dataset.page !== page; });
  window.scrollTo(0,0);
  document.querySelectorAll("[data-auth]").forEach(item => { item.hidden = !state.token; });
  document.querySelectorAll("[data-guest]").forEach(item => { item.hidden = !!state.token; });
  const navPage = page === "member" ? "feed" : page === "event" ? "events" : page;
  document.querySelectorAll("nav a").forEach(a => { if (a.hash === "#"+navPage) a.setAttribute("aria-current","page"); else a.removeAttribute("aria-current"); });
  if (state.ready) {
    if (page === "feed") await loadFeed();
    if (page === "member") await loadMember(Number(route.split("/")[1]));
    if (page === "event") await loadEvent(Number(route.split("/")[1]));
  }
}

async function signedIn(auth, page = "feed") {
  state.token = auth.accessToken;
  sessionStorage.setItem("sportsBuddyToken", state.token);
  await loadData();
  if (location.hash === "#" + page) await showPage();
  else location.hash = "#" + page;
}

$("#login-form").addEventListener("submit", event => {
  event.preventDefault();
  submit(event.currentTarget, async data => {
    await signedIn(await api("Auth/login", "POST", { email: data.get("email"), password: data.get("password") }));
    $("#login-form").reset();
  });
});

$("#register-form").addEventListener("submit", event => {
  event.preventDefault();
  submit(event.currentTarget, async data => {
    const auth = await api("Auth/register-with-address", "POST", {
      name: data.get("name"), email: data.get("email"), password: data.get("password"),
      preferredRangeKm: rangeValue(data.get("preferredRangeKm")),
      address: { country: data.get("country"), city: data.get("city"), street: data.get("street"), postcode: data.get("postcode") }
    });
    await signedIn(auth, "profile");
    $("#register-form").reset();
    notice("Account created. Add sports to find players, and availability to find events.");
  });
});

$("#range-form").addEventListener("submit", event => {
  event.preventDefault();
  submit(event.currentTarget, async data => {
    await api("Users/me/preferences", "PATCH", { preferredRangeKm: rangeValue(data.get("preferredRangeKm")) });
    state.user = await api("Users/me");
    renderProfile();
    notice("Preferred travel distance saved.");
  });
});

$("#availability-form").addEventListener("submit", event => {
  event.preventDefault();
  submit(event.currentTarget, async () => {
    const availabilities = [...$("#availability-rows").children].map(row => ({
      dayOfWeek: Number(row.querySelector(".slot-day").value),
      startTime: row.querySelector(".slot-start").value + ":00",
      endTime: row.querySelector(".slot-end").value + ":00"
    }));
    if (availabilities.some(slot => slot.endTime <= slot.startTime)) throw new Error("Each time slot must end after it starts on the same day.");
    await api("Users/me/availability", "PUT", availabilities);
    state.user = await api("Users/me");
    renderProfile();
    notice(availabilities.length ? "Availability saved for all your sports." : "Availability cleared for all your sports.");
  });
});

$("#skill-form").addEventListener("submit", event => {
  event.preventDefault();
  submit(event.currentTarget, async data => {
    const id = data.get("playerSkillId");
    const request = { skillId: Number(data.get("skillId")), skillLevel: Number(data.get("skillLevel")) };
    if (!id && state.playerSkills.some(p => p.skillId === request.skillId))
      throw new Error("This sport is already on your profile. Use Edit to change its level.");
    await api(id ? "PlayerSkills/" + id : "PlayerSkills", id ? "PUT" : "POST", request);
    await loadData();
    resetSkillForm();
    notice("Sport saved.");
  });
});

$("#new-sport-form").addEventListener("submit", event => {
  event.preventDefault();
  submit(event.currentTarget, async data => {
    const sport = await api("Skills", "POST", { name: data.get("name") });
    state.sports.push(sport);
    renderSports();
    $("#skill-choice").value = sport.id;
    $("#new-sport-form").reset();
    notice("Sport added. Choose your skill level.");
  });
});

function photoFile(form, field = "file") {
  const file = form.elements[field].files[0];
  if (file && (file.size > 5*1024*1024 || !["image/jpeg","image/png","image/webp"].includes(file.type)))
    throw new Error("Choose a JPEG, PNG or WebP photo up to 5 MB.");
  return file;
}
async function upload(path, file) { const body = new FormData(); body.append("file", file); return await api(path, "POST", body); }

$("#profile-form").addEventListener("submit", event => {
  event.preventDefault(); submit(event.currentTarget, async data => {
    await api("Users/me/profile","PATCH",{name:data.get("name"),bio:data.get("bio")||null});
    await loadData(); notice("Profile saved.");
  });
});
$("#profile-photo-form").addEventListener("submit", event => {
  event.preventDefault(); const form=event.currentTarget;
  submit(form, async () => { await upload("Users/me/photo",photoFile(form)); form.reset(); await loadData(); notice("Profile photo updated."); });
});
$("#event-form").addEventListener("submit", event => {
  event.preventDefault(); const form=event.currentTarget;
  submit(form, async data => {
    if (data.get("endDate") <= data.get("date")) throw new Error("The session must end after it starts.");
    const file=photoFile(form,"photo");
    let locationId=data.get("location");
    if(locationId === "new") {
      const address=await api("Locations","POST",{country:data.get("country"),city:data.get("city"),street:data.get("street"),postcode:data.get("postcode")});
      locationId=address.id;
    }
    const created=await api("Events","POST",{name:data.get("name"),date:data.get("date")+":00",endDate:data.get("endDate")+":00",
      skillId:Number(data.get("skillId")),skillLevel:Number(data.get("skillLevel")),location:Number(locationId),maxParticipants:Number(data.get("maxParticipants")),bio:data.get("bio")||null});
    form.reset();
    let message="Session created. You can edit it here or join it to add it to your Events tab.";
    if(file) { try { await upload("Events/"+created.id+"/photo",file); } catch(error) { message="Session created, but the photo was not saved: "+error.message; } }
    await loadData(); location.hash="#event/"+created.id; notice(message);
  });
});
$("#event-edit-form").addEventListener("submit", event => {
  event.preventDefault(); submit(event.currentTarget, async data => {
    if(data.get("endDate") <= data.get("date")) throw new Error("The session must end after it starts.");
    const item=state.activeEvent;
    await api("Events/"+item.id,"PUT",{name:data.get("name"),bio:data.get("bio")||null,skillId:Number(data.get("skillId")),skillLevel:Number(data.get("skillLevel")),
      maxParticipants:Number(data.get("maxParticipants")),location:item.location,date:data.get("date")+":00",endDate:data.get("endDate")+":00"});
    await loadData(); await loadEvent(item.id); notice("Session updated.");
  });
});
$("#event-photo-form").addEventListener("submit", event => {
  event.preventDefault(); const form=event.currentTarget;
  submit(form, async () => { await upload("Events/"+state.activeEvent.id+"/photo",photoFile(form)); form.reset(); await loadEvent(state.activeEvent.id); notice("Event photo updated."); });
});
$("#header-range").addEventListener("change", async event => {
  const select=event.currentTarget; select.disabled=true;
  try {
    await api("Users/me/preferences","PATCH",{preferredRangeKm:rangeValue(select.value)});
    state.user=await api("Users/me"); renderProfile();
    if(location.hash==="#feed") await loadFeed();
    notice("Travel distance updated.");
  } catch(error) { notice(error.message,true); } finally { select.disabled=false; }
});
$("#feed-sort").addEventListener("change",renderFeed);

$("#feed-form").addEventListener("submit", event => {
  event.preventDefault();
  submit(event.currentTarget, loadFeed);
});
$("#feed-sport").addEventListener("change", () => { loadFeed().catch(error => notice(error.message, true)); });
$("#event-location").addEventListener("change", eventAddressMode);
$("#add-slot").addEventListener("click", () => addSlot());
$("#cancel-skill").addEventListener("click", resetSkillForm);
$("#logout").addEventListener("click", signOut);
document.querySelectorAll("nav a, .brand, .auth-page a").forEach(link => link.addEventListener("click", () => notice()));
window.addEventListener("hashchange", () => { showPage().catch(error => notice(error.message, true)); });

async function start() {
  resetSkillForm();
  await showPage();
  if (state.token) {
    try { await loadData(); await showPage(); }
    catch (error) { notice(error.message, true); }
  }
}
start().catch(error => notice(error.message, true));

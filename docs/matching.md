# Matching rules

Player matching requires the selected sport, valid coordinates, and distance within both users' preferred ranges. Availability never filters players or changes their scores; users can connect even with different schedules or no saved schedule.

Player score: `0.625 * skill + 0.375 * location`. This preserves the original 5:3 skill/distance ratio, scaled to 100 after removing availability. Skill is `100 - 25 * abs(level difference)`. Location is `100 * (1 - distance / min(user ranges))`; identical coordinates with a zero range score 100.

Event matching still requires positive overlap with the user's weekly availability. Event score remains `0.5 * skill + 0.3 * location + 0.2 * availability`. Event availability is the percentage of event duration the user can attend. The distance limit is the user's preferred range. Already joined events are excluded from their feed.

Run `dotnet run --project tools/MatchingDemo --configuration AvailabilityFix` from the repository root to verify players with missing or non-overlapping schedules remain eligible, changing/clearing the current user's schedule leaves player scores unchanged, and events still require overlapping availability. Supply a running API base URL as the final argument to include HTTP checks.

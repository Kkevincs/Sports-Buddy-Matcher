param([string]$BaseUrl = 'http://127.0.0.1:5201')
$ErrorActionPreference = 'Stop'
$citySlugs = 'amsterdam','rotterdam','the-hague','utrecht','eindhoven','groningen','tilburg','almere','breda','nijmegen'
foreach ($citySlug in $citySlugs) {
    $body = @{ email = "$citySlug.01@netherlands-seed.example.test"; password = 'Netherlands-2026!' } | ConvertTo-Json
    $login = Invoke-RestMethod "$BaseUrl/api/Auth/login" -Method Post -ContentType 'application/json' -Body $body
    $headers = @{ Authorization = 'Bearer ' + $login.accessToken }
    $me = Invoke-RestMethod "$BaseUrl/api/Users/me" -Headers $headers
    if ($me.profilePictureUrl -or $me.name -match 'demo') { throw "Unexpected profile details for $citySlug" }
    $sports = Invoke-RestMethod "$BaseUrl/api/Skills" -Headers $headers
    foreach ($sportName in 'Soccer','Basketball','Tennis','Running','Cycling') {
        $sport = $sports | Where-Object name -eq $sportName
        $feed = Invoke-RestMethod "$BaseUrl/api/Matches/feed?skillId=$($sport.id)" -Headers $headers
        $players = @($feed | Where-Object { $_.type -eq 'user' })
        $events = @($feed | Where-Object { $_.type -eq 'event' -and $_.pictureUrl -like '/images/netherlands/*' })
        if ($players.Count -lt 11 -or $events.Count -lt 1) { throw "Missing local matches for $citySlug / $sportName" }
        $photo = Invoke-WebRequest ($BaseUrl + $events[0].pictureUrl) -UseBasicParsing
        if ($photo.StatusCode -ne 200 -or $photo.Headers.'Content-Type' -notlike 'image/png*') { throw 'Event photo did not load.' }
    }
    Write-Output "$citySlug : login, photo-free profile, and player/event feeds for all five sports verified."
}

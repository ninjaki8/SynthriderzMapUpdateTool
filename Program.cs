using Spectre.Console;
using SynthriderzMapUpdateTool;

// Console setup
var console = new ConsoleSetup("Synthriders Custom Songs Update CLI Tool");

// App info
AnsiConsole.Write(new FigletText("Synthriderz.com").Centered().Color(Color.Purple));
AnsiConsole.MarkupLine("\n[cornflowerblue][[INF]][/] [green]Synthriders Custom Songs Update CLI Tool[/]");
AnsiConsole.MarkupLine("[cornflowerblue][[INF]][/] [green]Ninjaki8[/]");

// Init vars
var deviceManager = new DeviceManager();
string elapsedMilliseconds;

// Start adb server
AnsiConsole.Status()
    .Start("Initializing adb daemon...", ctx =>
    {
        TimeMeasurement.Start();
        var status = deviceManager.StartAdbServer();
        elapsedMilliseconds = TimeMeasurement.ElapsedMilliseconds();
        AnsiConsole.MarkupLineInterpolated($"[mediumpurple2][[LOG]][/] [green]{status}[/] [mediumpurple2]{elapsedMilliseconds}[/]");
    });

// Get connected devices
var adbDevices = deviceManager.GetAdbDevices();
if (adbDevices.Count == 0)
{
    AnsiConsole.MarkupLineInterpolated($"[red][[SYS]][/] No devices connected");
    console.ExitApplication();
}
AnsiConsole.MarkupLineInterpolated($"[mediumpurple2][[LOG]][/] [green]Found {adbDevices.Count} connected device(s), awaiting user selection...[/]");

// Select device model prompt
string deviceModel = AnsiConsole
    .Prompt(new SelectionPrompt<string>()
    .Title("[grey50]---------------------------------------------------------------[/]\n" +
    "[red][[SYS]][/] Select device...\n" +
    "[cornflowerblue][[INF]][/] [grey62]Use Up/Down arrows to navigate, Spacebar/Enter to select)[/]")
    .AddChoices(adbDevices));

// Get device by selected model
var device = deviceManager.GetDeviceByModel(deviceModel);
if (!device)
{
    AnsiConsole.MarkupLine("[red][[SYS]][/] {0} Not Found!", deviceModel);
    console.ExitApplication();
}

AnsiConsole.MarkupLineInterpolated($"[mediumpurple2][[LOG]][/] [green]{deviceModel} selected[/]");

await AnsiConsole.Status()
    .StartAsync("Checking for synthriders custom songs folder...", async ctx =>
    {
        // Check if CustomSongs exists
        TimeMeasurement.Start();
        string customSongsPath = await deviceManager.GetSynthFolder();
        if (string.IsNullOrEmpty(customSongsPath))
        {
            AnsiConsole.MarkupLine("[red][[SYS]][/] Folder not found!");
            console.ExitApplication();
        }
        elapsedMilliseconds = TimeMeasurement.ElapsedMilliseconds();
        AnsiConsole.MarkupLineInterpolated($"[mediumpurple2][[LOG]][/] [green]Custom songs folder found[/] [mediumpurple2]{elapsedMilliseconds}[/]");

        // Get current beatmaps on device
        TimeMeasurement.Start();
        ctx.Status("Scanning CustomSongs folder...");
        var beatmaps = await deviceManager.GetSynthFiles(customSongsPath);
        elapsedMilliseconds = TimeMeasurement.ElapsedMilliseconds();
        AnsiConsole.MarkupLineInterpolated($"[mediumpurple2][[LOG]][/] [green]{beatmaps.Count} beatmap(s) found[/] [mediumpurple2]{elapsedMilliseconds}[/]");

        // Get beatmaps from synthriderz.com
        TimeMeasurement.Start();
        ctx.Status("Fetching data from synthriderz.com...");
        var synthApi = new SynthriderzApiData();
        var apiPageList = await synthApi.DownloadJsonPages();
        elapsedMilliseconds = TimeMeasurement.ElapsedMilliseconds();
        AnsiConsole.MarkupLineInterpolated($"[mediumpurple2][[LOG]][/] [green]{apiPageList.Count} pages received[/] [mediumpurple2]{elapsedMilliseconds}[/]");

        // Check for new maps
        TimeMeasurement.Start();
        ctx.Status("Checking for new beatmaps...");
        var beatmapManager = new BeatmapManager(beatmaps);
        var count = beatmapManager.CheckForNewBeatmaps(apiPageList);
        elapsedMilliseconds = TimeMeasurement.ElapsedMilliseconds();
        if (count > 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[mediumpurple2][[LOG]][/] [green]{count} new beatmaps found, added to queue[/] [mediumpurple2]{elapsedMilliseconds}[/]");

            // Download new maps
            TimeMeasurement.Start();
            ctx.Status("Downloading new beatmaps...");
            await beatmapManager.DownloadQueue(customSongsPath);
            elapsedMilliseconds = TimeMeasurement.ElapsedMilliseconds();
            AnsiConsole.MarkupLineInterpolated($"[mediumpurple2][[LOG]][/] [green]Download complete[/] [mediumpurple2]{elapsedMilliseconds}[/]");
        }

        AnsiConsole.MarkupLine("[mediumpurple2][[LOG]][/] [green]You are up to date! Have a nice day :)[/]");
    });

console.ExitApplication();
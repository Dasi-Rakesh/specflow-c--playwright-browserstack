using NUnit.Framework;
//[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace VScodeSpecflow.Hooks
{
    using BoDi;
    using LLAutomation.Util;

    using Microsoft.Playwright;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using System;
    using System.Diagnostics;
    using System.IO.Compression;
    using Newtonsoft.Json;
    using TechTalk.SpecFlow;

    [Binding]
    public class Hooks
    {
        public IPage? _page;
        public IBrowser? _browser;
        public IPlaywright _playwright;
        public IBrowserContext? _BrowserContext;
        public IBrowserType? _BrowserType;
        public Dictionary<string, object> pagename;
        public Type? type = null;
        CommonStepFunction Commonstepfunctionobject = null;
        public static string? reportPath = Directory.GetParent(@"../../../").FullName + Path.DirectorySeparatorChar + "Reports" + Path.DirectorySeparatorChar + "Report_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + Path.DirectorySeparatorChar;
        public static string? zipPath = Directory.GetParent(@"../../../").FullName + Path.DirectorySeparatorChar + "Reports" + Path.DirectorySeparatorChar + "Report_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".zip";
        public static string videprecordingPath = System.IO.Directory.GetParent(@"../../../").FullName + Path.DirectorySeparatorChar + "RecordedVideo" + Path.DirectorySeparatorChar;

        public static string? DeviceType;
        public static string? Devicename;
        private readonly IObjectContainer _objectContainer;
        public readonly ScenarioContext _scenarioContext;

        protected string? configFile = Directory.GetParent(@"../../../").FullName + Path.DirectorySeparatorChar + "Hooks" + Path.DirectorySeparatorChar + "single.conf.json";
        //protected string configFile= "C:/Users/MTiwari/Source/Repos/PropertySearchIQ.Automation.UI/LLAutomation/Hooks/single.conf.json";


        public Hooks(IObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            _objectContainer = objectContainer;
            _scenarioContext = scenarioContext;
            
        }


        [BeforeScenario]
        public async Task<IPage> InvokeBrowserBeforeScenario()
        {
            // Get Configuration for correct profile
            string currentDirectory = Directory.GetCurrentDirectory();
            string path = Path.Combine(currentDirectory, configFile);
            JObject config = JObject.Parse(File.ReadAllText(path));
            Console.WriteLine(path);
            if (config is null)
                throw new Exception("Configuration not found!");

            // Get Environment specific capabilities
            JObject capabilitiesJsonArr = config.GetValue("environments") as JObject;

            // Get Common Capabilities
            JObject commonCapabilities = config.GetValue("capabilities") as JObject;

            // Directly set the BrowserStack credentials from the config file
            string username = config.GetValue("user").ToString();
            string accessKey = config.GetValue("key").ToString();
            capabilitiesJsonArr["browserstack.user"] = username;
            capabilitiesJsonArr["browserstack.key"] = accessKey;

            // Merge Capabilities
            capabilitiesJsonArr.Merge(commonCapabilities);

            string capsJson = JsonConvert.SerializeObject(capabilitiesJsonArr);
            string cdpUrl = "wss://cdp.browserstack.com/playwright?caps=" + Uri.EscapeDataString(capsJson);

            // Playwright Init code
            _playwright = await Playwright.CreateAsync();
            DeviceType = "desktop";

            // Open browser in Desktop
            if (DeviceType == "desktop")
            {
                _BrowserType = _playwright.Chromium;
                bool headlessoption = false;
                string Headless = Environment.GetEnvironmentVariable("headless", EnvironmentVariableTarget.Process);
                if (Headless == null)
                {
                    headlessoption = false;
                }
                else
                {
                    headlessoption = System.Convert.ToBoolean(Headless.ToLower());
                }

                _browser = await _playwright.Chromium.ConnectAsync(cdpUrl);

                _BrowserContext = await _browser.NewContextAsync(new()
                {
                    ViewportSize = new ViewportSize() { Width = 1920, Height = 911 },
                });
                await _BrowserContext.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write", "geolocation" });

                _page = await _BrowserContext.NewPageAsync();
                await _page.GotoAsync("https://www.linklogistics.com/");
                await Task.Delay(10000);
            }

            _objectContainer.RegisterInstanceAs(_page);

            return _page;
        }

        [AfterScenario]
        public async Task AfterScenario()
        {

            Thread.Sleep(1000);
            if (_BrowserContext != null)
            { await _BrowserContext?.CloseAsync()!; }
            if (_browser != null)
            { await _browser?.CloseAsync()!; }
            if (_playwright != null)
            { _playwright.Dispose(); }
        }

        [BeforeTestRun(Order = 1)]
        public static void DeleteAllPrevioudReport()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Logger.Error("beforeTestRun", $"Exception: {ex.Message}. "
                            + $" StackTrace: {ex.StackTrace}"
                            + $" Source: {ex.Source}"
                            + $" InnerException: {ex.InnerException}");
            }
        }   
    }
}


using HardwareMonitor32.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace HardwareMonitor32.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HardwareMonitorController : ControllerBase
    {
        private readonly Settings settings;

        public HardwareMonitorController(IOptions<Settings> settings)
        {
            this.settings = settings.Value;
        }

        // GET: HardwareMonitor/Status
        [HttpGet]
        [Route("[action]")]
        public IActionResult Status()
        {
            string? registryPath = this.settings.RegistryPath;

            if (String.IsNullOrEmpty(registryPath))
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Missing required settings key: ${nameof(registryPath)}");
            }

            Dictionary<string, string> monitorResults = new Dictionary<string, string>();
            try
            {
                RegistryKey? registryKey = null;
                if (registryPath.Contains("HKEY_CURRENT_USER"))
                {
                    string strippedKey = registryPath.Replace("HKEY_CURRENT_USER", "").Trim('\\');
                    registryKey = Registry.CurrentUser.OpenSubKey(strippedKey);
                }
                else if (registryPath.Contains("HKEY_LOCAL_MACHINE"))
                {
                    string strippedKey = registryPath.Replace("HKEY_LOCAL_MACHINE", "").Trim('\\');
                    registryKey = Registry.CurrentUser.OpenSubKey(strippedKey);
                }

                if (registryKey == null)
                {
                    throw new ArgumentNullException(nameof(registryKey));
                }

                using (RegistryKey key = registryKey)
                {
                    if (key != null)
                    {
                        string[] valueNames = key.GetValueNames();

                        for (int i = 0; i < valueNames.Length; i += 3)
                        {
                            string? label = key.GetValue(valueNames[i])?.ToString();
                            string? value = key.GetValue(valueNames[i + 1])?.ToString();
                            string? color = key.GetValue(valueNames[i + 2])?.ToString();

                            if (String.IsNullOrEmpty(label) || String.IsNullOrEmpty(value))
                            {
                                continue;
                            }
                            monitorResults.Add(label, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return this.Ok(monitorResults);
        }
    }
}
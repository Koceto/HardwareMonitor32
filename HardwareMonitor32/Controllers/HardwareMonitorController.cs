using HardwareMonitor32.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.Text.RegularExpressions;

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
        public IActionResult Status(bool wrapped)
        {
            string? registryPath = this.settings.RegistryPath;

            if (String.IsNullOrEmpty(registryPath))
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Missing required settings key: ${nameof(registryPath)}");
            }

            List<MonitorStat> monitorResults = new List<MonitorStat>();
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
                    registryKey = Registry.LocalMachine.OpenSubKey(strippedKey);
                }

                if (registryKey == null)
                {
                    throw new ArgumentNullException(nameof(registryKey));
                }

                using (RegistryKey key = registryKey)
                {
                    if (key != null)
                    {
                        string[] subkeys = key.GetValueNames();
                        string[] labels = subkeys.Where(sk => sk.StartsWith("label", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        string[] values = subkeys.Where(sk => sk.StartsWith("value", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        string[] colors = subkeys.Where(sk => sk.StartsWith("color", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                        string pattern = @"label(\d+)$";
                        RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;

                        for (int i = 0; i < labels.Length; i++)
                        {
                            MatchCollection matches = Regex.Matches(labels[i], pattern, options);
                            string? matchedValue = matches[0]?.Groups[1]?.Value;
                            if (String.IsNullOrEmpty(matchedValue))
                            {
                                continue;
                            }

                            string? currentValueName = values.FirstOrDefault(v => v.EndsWith(matchedValue));
                            if (String.IsNullOrEmpty(currentValueName))
                            {
                                continue;
                            }

                            string? currentColorName = colors.FirstOrDefault(v => v.EndsWith(matchedValue));

                            string? label = key.GetValue(labels[i])?.ToString();
                            string? value = key.GetValue(currentValueName)?.ToString();
                            string? color = key.GetValue(currentColorName)?.ToString();
                            if (String.IsNullOrEmpty(label) || String.IsNullOrEmpty(value))
                            {
                                continue;
                            }

                            monitorResults.Add(new MonitorStat()
                            {
                                Name = label,
                                Value = value,
                                Color = color ?? String.Empty
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            if (wrapped)
            {
                return this.Ok(new HWInfo() { Stats = monitorResults });
            }
            return this.Ok(monitorResults);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tesseract;
using static dotnet9_jwt_concept.Models.OcrModels;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using dotnet9_jwt_concept.Models.Core;
using Octokit;

namespace dotnet9_jwt_concept.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : Controller
    {
        // GET: /api/default/health-check
        [HttpGet("health-check")]
        public async Task<ActionResult<object>> defaultAsync()
        {
            var payload = new
            {
                timeStamp = DateTime.UtcNow,
                statusCode = 200,
                message = "Api is running"
            };
            return Ok(payload);
        }

        [HttpGet]
        public async Task<ActionResult<object>> OcrAsync([FromBody] OcrRequest param)
        {
            // var imagePath = "slip.jpg"; // เปลี่ยนเป็น path ของใบเสร็จ
            using var engine = new TesseractEngine(@"./tesseract-ocr/tessdata", "tha+eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(param.ImagePath);
            using var page = engine.Process(img);

            //Console.WriteLine("ข้อความ OCR:");
            //Console.WriteLine(page.GetText());

            var text = page.GetText();
            var payload = new
            {
                timeStamp = DateTime.UtcNow,
                statusCode = 200,
                message = text
            };
            return Ok(payload);
        }

        [HttpPost("slip")]
        public async Task<IActionResult> OcrSlipAsync([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponseFactory.Fail(
                       message: "File is empty",
                       errorCode: "NotfoundFile"
                   ));

            // OCR
            string ocrTextRaw;
            string ocrText;
            await using (var stream = file.OpenReadStream())
            using (var engine = new TesseractEngine(@"./tesseract-ocr/tessdata", "tha+eng", EngineMode.Default))
            using (var img = Pix.LoadFromMemory(ToBytes(stream)))
            using (var page = engine.Process(img))
            {
                ocrTextRaw = page.GetText();
            }

            ocrText = Normalize(ocrTextRaw);

            // โหลด templates จาก JSON
            var templateJsonPath = Path.Combine(@"./tesseract-ocr", "bank_templates.json");
            if (!System.IO.File.Exists(templateJsonPath))
                return BadRequest(ApiResponseFactory.Fail(
                          message: "Template config not found",
                          errorCode: "NotfoundTemplate"
                      ));

            //var json = await System.IO.File.ReadAllTextAsync(templateJsonPath);
            //var templates = JsonSerializer.Deserialize<List<SlipTemplate>>(json);
            decimal amountTotal = 0;
            if (!ocrText.Contains("จํานวน"))
            {
                return BadRequest(ApiResponseFactory.Fail(
                           message: "ไม่พบจํานวนเงิน Contains",
                           errorCode: "NotfoundAmount"
                       ));

            }

            var match = Regex.Match(ocrText, @"จํานวน\s*([\d,]+\.\d{2})");
            if (!match.Success)
            {
                return BadRequest(ApiResponseFactory.Fail(
                          message: "ไม่พบจํานวนเงิน Regex",
                          errorCode: "NotfoundAmount"
                      ));
            }

            string amountStr = match.Groups[1].Value; // เช่น "200.00"
            if (decimal.TryParse(amountStr.Replace(",", ""), out var amount))
            {
                amountTotal = amount;
            }

            var result = new
            {
                amount
            };

            return Ok(ApiResponseFactory.Ok(
                    message: "ocr check slip success",
                    data: result
                )
            );
        }

        // helper
        private static byte[] ToBytes(Stream s)
        {
            using var ms = new MemoryStream();
            s.CopyTo(ms); return ms.ToArray();
        }

        private static DateTime? ParseCustomDateTime(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // input ตัวอย่าง: "01ก.ค.2568-0922"
            // แยกวันที่ กับ เวลาออกจากกัน (ใช้ - หรือ –)
            var parts = input.Split(new char[] { '-', '–' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return null;

            string datePart = parts[0]; // "01ก.ค.2568"
            string timePart = parts[1]; // "0922"

            // แทรกเว้นวรรคให้ตรงรูปแบบที่ DateTime.ParseExact รับได้ เช่น "01 ก.ค. 2568"
            var dateWithSpaces = Regex.Replace(datePart, @"(\d{1,2})([ก-ฮ]{1,3})\.(\d{4})", "$1 $2 $3");

            // แทรก colon เวลาจาก "0922" → "09:22"
            if (timePart.Length != 4) return null;
            string timeFormatted = timePart.Insert(2, ":");

            string dateTimeStr = $"{dateWithSpaces} {timeFormatted}";

            // Parse ด้วย Thai culture
            if (DateTime.TryParseExact(dateTimeStr,
                new[] { "dd MMM yyyy HH:mm", "d MMM yyyy HH:mm" },
                new CultureInfo("th-TH"),
                DateTimeStyles.None,
                out var dt))
            {
                return dt;
            }

            return null;
        }


        private static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            s = s.Normalize(NormalizationForm.FormC);
            
            s = Regex.Replace(s, @"(?<=\S) (?=\S)", "");

            s = Regex.Replace(s, @"[@\|/\\\[\]\(\):]", "");

            return s;
        }

        private static bool IsTemplateMatch(SlipTemplate t, string ocr, int defaultRequireHits = 1)
        {
            string text = Normalize(ocr);

            int hits = t.Detect.Count(d =>
                Regex.IsMatch(text,
                              Regex.Escape(Normalize(d)),
                              RegexOptions.IgnoreCase));

            int needed = t.MinHits ?? defaultRequireHits;
            needed = Math.Min(needed, t.Detect.Count());
            return hits >= needed;
        }

        private string NormalizeOcrText(string rawText)
        {
            // ลบช่องว่างระหว่างตัวอักษรไทย/อังกฤษ (เว้นไว้เฉพาะระหว่างคำ)
            // ใช้ heuristic ว่า ถ้ามีช่องว่างทุกตัวติดกัน ให้รวมมัน
            var lines = rawText.Split('\n');
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                // ลบช่องว่างระหว่างอักษรเดี่ยว ๆ
                var cleanedLine = Regex.Replace(line, @"(?<=\S) (?=\S)", "");
                sb.AppendLine(cleanedLine);
            }

            return sb.ToString();
        }

        public class SlipTemplate
        {
            public string Name { get; set; } = "";
            public int? MinHits { get; set; }
            public string[] Detect { get; set; } = Array.Empty<string>();
            public Dictionary<string, FieldDefinition> Fields { get; set; } = new();

            public SlipInfo Parse(string text)
            {
                var info = new SlipInfo();

                foreach (var kv in Fields)
                {
                    string field = kv.Key;
                    FieldDefinition rule = kv.Value;
                    string? extracted = null;

                    if (rule.Type == "fixed")
                    {
                        // กรณีค่าคงที่ เช่น Status
                        extracted = rule.Value;
                    }
                    else if (rule.Type == "regex" && !string.IsNullOrWhiteSpace(rule.Pattern))
                    {
                        // ใช้ regex จับข้อมูลจากข้อความ OCR
                        var m = Regex.Match(text, rule.Pattern, RegexOptions.Singleline | RegexOptions.Multiline);
                        if (m.Success && m.Groups.Count > 1)
                        {
                            extracted = m.Groups[1].Value.Trim();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(extracted))
                        continue;

                    var prop = typeof(SlipInfo).GetProperty(field);
                    if (prop == null)
                        continue;

                    object? value = extracted;

                    if (prop.PropertyType == typeof(DateTime?))
                    {
                        var dt = ParseCustomDateTime(extracted);
                        if (dt != null)
                            value = dt;
                    }
                    else if (prop.PropertyType == typeof(decimal?))
                    {
                        // ลบ comma ก่อนแปลง
                        var decStr = extracted.Replace(",", "");
                        if (decimal.TryParse(decStr, out var dec))
                            value = dec;
                    }

                    prop.SetValue(info, value);
                }

                return info;
            }

            private static DateTime? ParseCustomDateTime(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return null;

                // ตัวอย่าง input: "01ก.ค.2568-0922"
                var parts = input.Split(new char[] { '-', '–' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    return null;

                string datePart = parts[0]; // "01ก.ค.2568"
                string timePart = parts[1]; // "0922"

                // แทรกเว้นวรรคเพื่อให้ตรงกับรูปแบบ dd MMM yyyy (ไทย)
                var dateWithSpaces = Regex.Replace(datePart, @"(\d{1,2})([ก-ฮ]{1,3})\.(\d{4})", "$1 $2 $3");

                // แปลงเวลาจาก 0922 → 09:22
                if (timePart.Length != 4) return null;
                string timeFormatted = timePart.Insert(2, ":");

                string dateTimeStr = $"{dateWithSpaces} {timeFormatted}";

                if (DateTime.TryParseExact(dateTimeStr,
                    new[] { "dd MMM yyyy HH:mm", "d MMM yyyy HH:mm" },
                    new CultureInfo("th-TH"),
                    DateTimeStyles.None,
                    out var dt))
                {
                    return dt;
                }

                return null;
            }
        }

        public class FieldDefinition
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public string Pattern { get; set; }
        }

        public class SlipInfo
        {
            public string? Status { get; set; }
            public string? Reference { get; set; }
            public DateTime? DateTime { get; set; }
            public string? FromName { get; set; }
            public string? ToName { get; set; }
            public decimal? Amount { get; set; }
        }
    }
}

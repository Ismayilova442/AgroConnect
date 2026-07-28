using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AgroConnect.Web.Controllers
{
    public class AICoachController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AICoachController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // MÜVƏQQƏTİ: AI analiz funksiyası hazırda deaktivdir.
        // Real API-ə qoşmaq üçün aşağıdakı "return View(\"ComingSoon\");" sətrini silib,
        // altındaki şərh (comment) blokunu aktivləşdirin.
        [HttpPost]
        public IActionResult AnalyzeImage(IFormFile plantImage)
        {
            return View("ComingSoon");

            /*
            if (plantImage == null || plantImage.Length == 0)
            {
                ViewBag.Error = "Zəhmət olmasa bitkinin şəklini yükləyin.";
                return View("Index");
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(plantImage.ContentType))
            {
                ViewBag.Error = "Yalnız JPG, PNG və ya WEBP formatında şəkil yükləyin.";
                return View("Index");
            }

            if (plantImage.Length > 8 * 1024 * 1024)
            {
                ViewBag.Error = "Şəkil çox böyükdür (maksimum 8 MB).";
                return View("Index");
            }

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await plantImage.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            var base64Image = Convert.ToBase64String(imageBytes);
            var dataUrl = $"data:{plantImage.ContentType};base64,{base64Image}";

            try
            {
                ViewBag.AnalysisResult = await AnalyzeWithAiAsync(dataUrl);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Analiz zamanı xəta baş verdi. Bir az sonra yenidən cəhd edin.";
                Console.WriteLine("AI Analiz xətası: " + ex.Message);
            }

            return View("Index");
            */
        }

        private async Task<string> AnalyzeWithAiAsync(string imageDataUrl)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("OpenAI API açarı konfiqurasiya edilməyib (appsettings.json -> OpenAI:ApiKey).");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            client.Timeout = TimeSpan.FromSeconds(45);

            const string systemPrompt = @"Sən təcrübəli bir aqronomsan. Sənə bir şəkil göndəriləcək.

QAYDA 1: Əgər şəkildə HƏQİQƏTƏN bitki, yarpaq, gövdə, meyvə və ya tərəvəz görünürsə:
- Bitkinin mümkün xəstəliyini/zərərvericisini müəyyən et (sağlamdırsa 'sağlam görünür' de)
- Səbəbini qısa izah et
- Təbii və kimyəvi mübarizə tövsiyələri ver
Cavabı YALNIZ bu HTML formatında qaytar, başqa heç nə yazma:
<h5><i class='bi bi-bug text-danger'></i> Tapılan Problem: [problem adı]</h5>
<p>[qısa izahat, 2-3 cümlə]</p>
<hr/>
<h6><i class='bi bi-capsule text-success'></i> Tövsiyə Edilən Tədbirlər:</h6>
<ul>
<li>[tədbir 1]</li>
<li>[tədbir 2]</li>
<li>[tədbir 3]</li>
</ul>
<p class='text-muted small'>* Qeyd: Bu AI analizidir. Dəqiq diaqnoz üçün aqronomla məsləhətləşin.</p>

QAYDA 2: Əgər şəkildə bitki YOXDURSA (insan, heyvan, əşya, mətn, bulanıq/qaranlıq şəkil və s.):
Cavabı YALNIZ bu formatda qaytar, başqa heç nə yazma:
<div class='alert alert-warning mb-0'><i class='bi bi-exclamation-triangle'></i> Bu şəkildə bitki aşkar edilmədi. Zəhmət olmasa bitkinin (yarpaq, gövdə və ya meyvə) aydın və yaxın çəkilmiş şəklini yükləyin.</div>

Cavabı YALNIZ Azərbaycan dilində ver. HTML tag-larından başqa heç bir markdown (** və ya #) istifadə etmə.";

            var requestBody = new
            {
                model = "gpt-5-mini",
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "Bu şəkli analiz et." },
                            new { type = "image_url", image_url = new { url = imageDataUrl } }
                        }
                    }
                },
                max_tokens = 700,
                temperature = 0.4
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API xətası ({response.StatusCode}): {responseString}");
            }

            using var doc = JsonDocument.Parse(responseString);
            var messageContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return messageContent ?? "<div class='alert alert-warning'>Nəticə alınmadı, yenidən cəhd edin.</div>";
        }
    }
}
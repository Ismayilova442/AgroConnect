using Microsoft.AspNetCore.Mvc;

namespace AgroConnect.Web.Controllers
{
    public class AICoachController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(IFormFile plantImage)
        {
            if (plantImage == null || plantImage.Length == 0)
            {
                ViewBag.Error = "Zəhmət olmasa bitkinin şəklini yükləyin.";
                return View("Index");
            }

            // Süni intellekt analizi simulyasiyası (2 saniyə gözləmə)
            await Task.Delay(2000);

            // Mock cavab - Əslində burada OpenAI Vision API və ya Gemini API-ə şəkil göndərilir
            // və gələn cavab parse edilir.
            ViewBag.AnalysisResult = @"
                <h5><i class='bi bi-bug text-danger'></i> Tapılan Problem: Yarpaq Qıvrılması (Leaf Curl)</h5>
                <p>Bu xəstəlik əsasən zərərverici həşəratlar (məs. mənənə) vasitəsilə yayılır və yarpaqların deformasiyasına səbəb olur.</p>
                <hr/>
                <h6><i class='bi bi-capsule text-success'></i> Tövsiyə Edilən Tədbirlər:</h6>
                <ul>
                    <li>Təbii mübarizə: Neem yağı (Neem oil) və ya sabunlu su məhlulu istifadə edin.</li>
                    <li>Kimyəvi mübarizə: İmidakloprid və ya Asetamiprid tərkibli insektisidlər (Zərərvericilər üçün) istifadə oluna bilər.</li>
                    <li>Xəstə yarpaqları qoparıb sahədən uzaqlaşdırın.</li>
                </ul>
                <p class='text-muted small'>* Qeyd: Bu AI analizidir. Dəqiq diaqnoz üçün aqronomla məsləhətləşin.</p>
            ";

            return View("Index");
        }
    }
}

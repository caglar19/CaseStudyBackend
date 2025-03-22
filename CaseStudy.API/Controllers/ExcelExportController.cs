using CaseStudy.Application.Constants;
using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.BayTahmin;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LicenseContext = OfficeOpenXml.LicenseContext;
using Microsoft.Extensions.Logging;

namespace CaseStudy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelExportController : ControllerBase
    {
        private readonly IBayTahminService _bayTahminService;
        private readonly ILogger<ExcelExportController> _logger;

        public ExcelExportController(IBayTahminService bayTahminService, ILogger<ExcelExportController> logger)
        {
            _bayTahminService = bayTahminService;
            _logger = logger;
        }

        /// <summary>
        /// Belirtilen tarihteki maç fikstürlerini Excel dosyasına aktarır
        /// </summary>
        /// <param name="date">Tarih (YYYY-MM-DD formatında)</param>
        /// <param name="filterByLeague">Sadece seçili ligleri getir (varsayılan: true)</param>
        /// <returns>Excel dosyası</returns>
        [HttpGet("maclar-excel")]
        public async Task<IActionResult> MaclariExceleAktar(string date, bool filterByLeague = true)
        {
            try
            {
                _logger.LogInformation("Maç verileri getiriliyor. Tarih: {Date}, Filtreleme: {Filter}", date, filterByLeague);

                // Maç verilerini getir
                var maclar = await _bayTahminService.GetFixturesAsync(date);
                
                _logger.LogInformation("Maç verileri alındı. Toplam maç sayısı: {Count}", maclar?.Count ?? 0);

                if (maclar == null || maclar.Count == 0)
                {
                    _logger.LogWarning("Belirtilen tarihte maç bulunamadı. Tarih: {Date}", date);
                    return NotFound("Belirtilen tarihte maç bulunamadı.");
                }

                // Maçları filtrele
                if (filterByLeague)
                {
                    maclar = maclar.Where(m => 
                        LeagueConstants.SelectedCountries.Contains(m.League?.Country) && 
                        LeagueConstants.SelectedLeagues.TryGetValue(m.League?.Country, out var leagues) && 
                        leagues.Contains(m.League?.Name)
                    ).ToList();

                    _logger.LogInformation("Filtreleme sonrası maç sayısı: {Count}", maclar.Count);

                    if (maclar.Count == 0)
                    {
                        return NotFound("Seçili liglerde maç bulunamadı.");
                    }
                }

                // Excel paketi oluştur
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var paket = new ExcelPackage();
                var sayfa = paket.Workbook.Worksheets.Add("Maçlar");

                // Başlıkları ekle
                sayfa.Cells[1, 1].Value = "Tarih";
                sayfa.Cells[1, 2].Value = "Ülke";
                sayfa.Cells[1, 3].Value = "Lig";
                sayfa.Cells[1, 4].Value = "Ev Sahibi";
                sayfa.Cells[1, 5].Value = "Deplasman";
                sayfa.Cells[1, 6].Value = "Kazanan Tahmini";
                sayfa.Cells[1, 7].Value = "Kazanma Yüzdesi (Ev)";
                sayfa.Cells[1, 8].Value = "Beraberlik Yüzdesi";
                sayfa.Cells[1, 9].Value = "Kazanma Yüzdesi (Dep)";
                sayfa.Cells[1, 10].Value = "Form Karşılaştırması (Ev/Dep)";
                sayfa.Cells[1, 11].Value = "Hücum Karşılaştırması (Ev/Dep)";
                sayfa.Cells[1, 12].Value = "Savunma Karşılaştırması (Ev/Dep)";
                sayfa.Cells[1, 13].Value = "Skor (İY)";
                sayfa.Cells[1, 14].Value = "Skor";
                sayfa.Cells[1, 15].Value = "Lig Sırası (Ev)";
                sayfa.Cells[1, 16].Value = "Lig Sırası (Dep)";
                sayfa.Cells[1, 17].Value = "Gol (Ev)";
                sayfa.Cells[1, 18].Value = "Gol (Dep)";
                sayfa.Cells[1, 19].Value = "Stadyum";
                sayfa.Cells[1, 20].Value = "Şehir";
                sayfa.Cells[1, 21].Value = "Hakem";
                sayfa.Cells[1, 22].Value = "Durum";
                sayfa.Cells[1, 23].Value = "Tahmin";
                sayfa.Cells[1, 24].Value = "Kazanma %";
                sayfa.Cells[1, 25].Value = "Beraberlik %";
                sayfa.Cells[1, 26].Value = "Form";
                sayfa.Cells[1, 27].Value = "Hücum";
                sayfa.Cells[1, 28].Value = "Savunma";
                sayfa.Cells[1, 29].Value = "H2H";
                sayfa.Cells[1, 30].Value = "Toplam";

                // Başlık stilini ayarla
                var baslikAraligi = sayfa.Cells[1, 1, 1, 30];
                baslikAraligi.Style.Font.Bold = true;
                baslikAraligi.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                baslikAraligi.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Verileri ekle
                int satir = 2;
                foreach (var mac in maclar.OrderBy(m => m.League?.Country).ThenBy(m => m.League?.Name).ThenBy(m => m.Info?.Date))
                {
                    _logger.LogDebug("Maç verisi işleniyor. Maç ID: {Id}, Tarih: {Date}, Ülke: {Country}, Lig: {League}", 
                        mac.Info?.Id ?? 0, 
                        mac.Info?.Date.ToString("dd.MM.yyyy HH:mm") ?? "-",
                        mac.League?.Country ?? "-",
                        mac.League?.Name ?? "-");

                    // Temel bilgiler
                    sayfa.Cells[satir, 1].Value = mac.Info?.Date.ToString("dd.MM.yyyy HH:mm");
                    sayfa.Cells[satir, 2].Value = mac.League?.Country;
                    sayfa.Cells[satir, 3].Value = mac.League?.Name;
                    sayfa.Cells[satir, 4].Value = mac.Teams?.Home?.Name;
                    sayfa.Cells[satir, 5].Value = mac.Teams?.Away?.Name;

                    // Tahmin verilerini al
                    var prediction = await _bayTahminService.GetPredictionAsync(mac.Info.Id);
                    if (prediction != null)
                    {
                        sayfa.Cells[satir, 6].Value = prediction.Predictions?.Winner?.Name ?? "-";
                        sayfa.Cells[satir, 7].Value = prediction.Predictions?.Percent?.Home ?? "-";
                        sayfa.Cells[satir, 8].Value = prediction.Predictions?.Percent?.Draw ?? "-";
                        sayfa.Cells[satir, 9].Value = prediction.Predictions?.Percent?.Away ?? "-";
                        sayfa.Cells[satir, 10].Value = $"{prediction.Comparison?.Form?.Home ?? "-"}/{prediction.Comparison?.Form?.Away ?? "-"}";
                        sayfa.Cells[satir, 11].Value = $"{prediction.Comparison?.Att?.Home ?? "-"}/{prediction.Comparison?.Att?.Away ?? "-"}";
                        sayfa.Cells[satir, 12].Value = $"{prediction.Comparison?.Def?.Home ?? "-"}/{prediction.Comparison?.Def?.Away ?? "-"}";
                    }
                    else
                    {
                        sayfa.Cells[satir, 6].Value = "-";
                        sayfa.Cells[satir, 7].Value = "-";
                        sayfa.Cells[satir, 8].Value = "-";
                        sayfa.Cells[satir, 9].Value = "-";
                        sayfa.Cells[satir, 10].Value = "-";
                        sayfa.Cells[satir, 11].Value = "-";
                        sayfa.Cells[satir, 12].Value = "-";
                    }

                    sayfa.Cells[satir, 13].Value = mac.Score?.Halftime?.Home != null && mac.Score?.Halftime?.Away != null ? $"{mac.Score.Halftime.Home}-{mac.Score.Halftime.Away}" : "-";
                    sayfa.Cells[satir, 14].Value = mac.Score?.Fulltime?.Home != null && mac.Score?.Fulltime?.Away != null ? $"{mac.Score.Fulltime.Home}-{mac.Score.Fulltime.Away}" : "-";
                    sayfa.Cells[satir, 15].Value = mac.Info?.LeagueStanding?.HomePosition;
                    sayfa.Cells[satir, 16].Value = mac.Info?.LeagueStanding?.AwayPosition;
                    sayfa.Cells[satir, 17].Value = mac.Info?.LeagueStanding != null ? 
                        $"{mac.Info.LeagueStanding.HomeGoalsFor}-{mac.Info.LeagueStanding.HomeGoalsAgainst}" : "-";
                    sayfa.Cells[satir, 18].Value = mac.Info?.LeagueStanding != null ? 
                        $"{mac.Info.LeagueStanding.AwayGoalsFor}-{mac.Info.LeagueStanding.AwayGoalsAgainst}" : "-";
                    sayfa.Cells[satir, 19].Value = mac.Info?.Venue?.Name;
                    sayfa.Cells[satir, 20].Value = mac.Info?.Venue?.City;
                    sayfa.Cells[satir, 21].Value = mac.Info?.Referee;
                    sayfa.Cells[satir, 22].Value = mac.Info?.Status?.Long;

                    // Tahmin verilerini al
                    var tahmin = await _bayTahminService.GetPredictionAsync(mac.Info.Id);
                    if (tahmin != null)
                    {
                        // Tahmin ve yüzdeler
                        sayfa.Cells[satir, 23].Value = tahmin.Predictions?.Advice;
                        
                        var homePercent = tahmin.Predictions?.Percent?.Home?.TrimEnd('%') ?? "0";
                        var drawPercent = tahmin.Predictions?.Percent?.Draw?.TrimEnd('%') ?? "0";
                        var awayPercent = tahmin.Predictions?.Percent?.Away?.TrimEnd('%') ?? "0";
                        
                        sayfa.Cells[satir, 24].Value = $"{homePercent}% - {awayPercent}%";
                        sayfa.Cells[satir, 25].Value = drawPercent + "%";

                        // Karşılaştırma verileri
                        if (tahmin.Comparison != null)
                        {
                            sayfa.Cells[satir, 26].Value = $"{tahmin.Comparison.Form?.Home ?? "0%"} - {tahmin.Comparison.Form?.Away ?? "0%"}";
                            sayfa.Cells[satir, 27].Value = $"{tahmin.Comparison.Att?.Home ?? "0%"} - {tahmin.Comparison.Att?.Away ?? "0%"}";
                            sayfa.Cells[satir, 28].Value = $"{tahmin.Comparison.Def?.Home ?? "0%"} - {tahmin.Comparison.Def?.Away ?? "0%"}";
                            sayfa.Cells[satir, 29].Value = $"{tahmin.Comparison.H2h?.Home ?? "0%"} - {tahmin.Comparison.H2h?.Away ?? "0%"}";
                            sayfa.Cells[satir, 30].Value = $"{tahmin.Comparison.Total?.Home ?? "0%"} - {tahmin.Comparison.Total?.Away ?? "0%"}";
                        }
                    }

                    // Kazanan takımı yeşil, kaybedeni kırmızı yap
                    if (mac.Score?.Fulltime?.Home != null && mac.Score?.Fulltime?.Away != null)
                    {
                        var homeGoals = mac.Score.Fulltime.Home ?? 0;
                        var awayGoals = mac.Score.Fulltime.Away ?? 0;

                        if (homeGoals > awayGoals)
                        {
                            sayfa.Cells[satir, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            sayfa.Cells[satir, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                            sayfa.Cells[satir, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            sayfa.Cells[satir, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                        }
                        else if (homeGoals < awayGoals)
                        {
                            sayfa.Cells[satir, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            sayfa.Cells[satir, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                            sayfa.Cells[satir, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            sayfa.Cells[satir, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                        }
                    }

                    // Alternatif satır renklendirmesi
                    if (satir % 2 == 0)
                    {
                        var aralik = sayfa.Cells[satir, 1, satir, 30];
                        aralik.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        aralik.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                    }

                    satir++;
                }

                // Sütunları otomatik boyutlandır
                sayfa.Cells.AutoFitColumns();

                // Tüm hücrelere border ekle
                var veriAraligi = sayfa.Cells[1, 1, satir, 30];
                veriAraligi.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Filtre ekle
                sayfa.Cells[1, 1, 1, 30].AutoFilter = true;

                // Dosya adı oluştur
                string dosyaAdi = $"Maclar_{date}_{(filterByLeague ? "SeciliLigler" : "TumLigler")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                _logger.LogInformation("Excel dosyası oluşturuldu. Dosya adı: {FileName}", dosyaAdi);

                // Excel içeriğini byte dizisine çevir
                var icerik = paket.GetAsByteArray();

                // Dosyayı kullanıcıya gönder
                return File(icerik, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", dosyaAdi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Maçlar Excel'e aktarılırken hata oluştu. Tarih: {Date}", date);
                return BadRequest($"Maçlar Excel'e aktarılırken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// İki takım arasındaki geçmiş maçları Excel'e aktarır
        /// </summary>
        /// <param name="team1Id">1. Takım ID</param>
        /// <param name="team2Id">2. Takım ID</param>
        /// <returns>Excel dosyası</returns>
        [HttpGet("head-to-head-excel")]
        public async Task<IActionResult> HeadToHeadExcel(int team1Id, int team2Id)
        {
            try
            {
                _logger.LogInformation("Head to head maç verileri getiriliyor. Takımlar: {Team1Id}-{Team2Id}", team1Id, team2Id);

                // Maç verilerini getir
                var maclar = await _bayTahminService.GetHeadToHeadFixturesAsync($"{team1Id}-{team2Id}");
                
                _logger.LogInformation("Head to head maç verileri alındı. Toplam maç sayısı: {Count}", maclar?.Count ?? 0);

                if (maclar == null || maclar.Count == 0)
                {
                    _logger.LogWarning("İki takım arasında maç bulunamadı. Takımlar: {Team1Id}-{Team2Id}", team1Id, team2Id);
                    return NotFound("İki takım arasında maç bulunamadı.");
                }

                // Excel paketi oluştur
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var paket = new ExcelPackage();
                var sayfa = paket.Workbook.Worksheets.Add("H2H Maçlar");

                // Başlıkları ekle
                sayfa.Cells[1, 1].Value = "Tarih";
                sayfa.Cells[1, 2].Value = "Sezon";
                sayfa.Cells[1, 3].Value = "Turnuva";
                sayfa.Cells[1, 4].Value = "Ev Sahibi";
                sayfa.Cells[1, 5].Value = "Deplasman";
                sayfa.Cells[1, 6].Value = "Skor (İY)";
                sayfa.Cells[1, 7].Value = "Skor";
                sayfa.Cells[1, 8].Value = "Uzatma";
                sayfa.Cells[1, 9].Value = "Penaltı";
                sayfa.Cells[1, 10].Value = "Stadyum";
                sayfa.Cells[1, 11].Value = "Şehir";
                sayfa.Cells[1, 12].Value = "Hakem";
                sayfa.Cells[1, 13].Value = "Durum";
                sayfa.Cells[1, 14].Value = "Şut";
                sayfa.Cells[1, 15].Value = "İsabetli Şut";
                sayfa.Cells[1, 16].Value = "Top Hakimiyeti";
                sayfa.Cells[1, 17].Value = "Korner";
                sayfa.Cells[1, 18].Value = "Ofsayt";
                sayfa.Cells[1, 19].Value = "Faul";
                sayfa.Cells[1, 20].Value = "Sarı Kart";
                sayfa.Cells[1, 21].Value = "Kırmızı Kart";
                sayfa.Cells[1, 22].Value = "Diziliş";
                sayfa.Cells[1, 23].Value = "İlk 11";
                sayfa.Cells[1, 24].Value = "Yedekler";

                // Başlık stilini ayarla
                var baslikAraligi = sayfa.Cells[1, 1, 1, 24];
                baslikAraligi.Style.Font.Bold = true;
                baslikAraligi.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                baslikAraligi.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Verileri ekle
                int satir = 2;
                foreach (var mac in maclar.OrderByDescending(m => m.Info?.Date))
                {
                    _logger.LogDebug("Maç verisi işleniyor. Maç ID: {Id}, Tarih: {Date}, Lig: {League}", 
                        mac.Info?.Id ?? 0, 
                        mac.Info?.Date.ToString("dd.MM.yyyy HH:mm") ?? "-",
                        mac.League?.Name ?? "-");

                    // Temel bilgiler
                    sayfa.Cells[satir, 1].Value = mac.Info?.Date.ToString("dd.MM.yyyy HH:mm");
                    sayfa.Cells[satir, 2].Value = mac.League?.Season.ToString();
                    sayfa.Cells[satir, 3].Value = $"{mac.League?.Name} ({mac.League?.Round})";
                    sayfa.Cells[satir, 4].Value = mac.Teams?.Home?.Name;
                    sayfa.Cells[satir, 5].Value = mac.Teams?.Away?.Name;

                    // Skorlar
                    string ilkYariSkor = "-";
                    if (mac.Score?.Halftime?.Home != null && mac.Score?.Halftime?.Away != null)
                    {
                        ilkYariSkor = $"{mac.Score.Halftime.Home}-{mac.Score.Halftime.Away}";
                    }
                    sayfa.Cells[satir, 6].Value = ilkYariSkor;

                    string macSkor = "-";
                    if (mac.Score?.Fulltime?.Home != null && mac.Score?.Fulltime?.Away != null)
                    {
                        macSkor = $"{mac.Score.Fulltime.Home}-{mac.Score.Fulltime.Away}";

                        // Kazananı renklendir
                        var cell = sayfa.Cells[satir, 7];
                        if (mac.Score.Fulltime.Home > mac.Score.Fulltime.Away)
                        {
                            cell.Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        }
                        else if (mac.Score.Fulltime.Home < mac.Score.Fulltime.Away)
                        {
                            cell.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        }
                    }
                    sayfa.Cells[satir, 7].Value = macSkor;

                    // Uzatma ve penaltı skorları
                    string uzatmaSkor = "-";
                    if (mac.Score?.Extratime?.Home != null && mac.Score?.Extratime?.Away != null)
                    {
                        uzatmaSkor = $"{mac.Score.Extratime.Home}-{mac.Score.Extratime.Away}";
                    }
                    sayfa.Cells[satir, 8].Value = uzatmaSkor;

                    string penaltiSkor = "-";
                    if (mac.Score?.Penalty?.Home != null && mac.Score?.Penalty?.Away != null)
                    {
                        penaltiSkor = $"{mac.Score.Penalty.Home}-{mac.Score.Penalty.Away}";
                    }
                    sayfa.Cells[satir, 9].Value = penaltiSkor;

                    // Diğer bilgiler
                    sayfa.Cells[satir, 10].Value = mac.Info?.Venue?.Name;
                    sayfa.Cells[satir, 11].Value = mac.Info?.Venue?.City;
                    sayfa.Cells[satir, 12].Value = mac.Info?.Referee;
                    sayfa.Cells[satir, 13].Value = mac.Info?.Status?.Long;

                    // Maç istatistiklerini al
                    var istatistikler = await _bayTahminService.GetFixtureStatisticsAsync(mac.Info.Id);
                    if (istatistikler?.Count > 0)
                    {
                        var homeStats = istatistikler.FirstOrDefault(s => s.Team?.Id == mac.Teams?.Home?.Id)?.Statistics;
                        var awayStats = istatistikler.FirstOrDefault(s => s.Team?.Id == mac.Teams?.Away?.Id)?.Statistics;

                        // İstatistikleri ekle
                        sayfa.Cells[satir, 14].Value = $"Şut: {homeStats?.FirstOrDefault(s => s.Type == "Total Shots")?.Value ?? "0"}-{awayStats?.FirstOrDefault(s => s.Type == "Total Shots")?.Value ?? "0"}";
                        sayfa.Cells[satir, 15].Value = $"İsabetli Şut: {homeStats?.FirstOrDefault(s => s.Type == "Shots on Goal")?.Value ?? "0"}-{awayStats?.FirstOrDefault(s => s.Type == "Shots on Goal")?.Value ?? "0"}";
                        sayfa.Cells[satir, 16].Value = $"Top Hakimiyeti: {homeStats?.FirstOrDefault(s => s.Type == "Ball Possession")?.Value ?? "0"}-{awayStats?.FirstOrDefault(s => s.Type == "Ball Possession")?.Value ?? "0"}";
                        sayfa.Cells[satir, 17].Value = $"Korner: {homeStats?.FirstOrDefault(s => s.Type == "Corner Kicks")?.Value ?? "0"}-{awayStats?.FirstOrDefault(s => s.Type == "Corner Kicks")?.Value ?? "0"}";
                        sayfa.Cells[satir, 18].Value = $"Ofsayt: {homeStats?.FirstOrDefault(s => s.Type == "Offsides")?.Value ?? "0"}-{awayStats?.FirstOrDefault(s => s.Type == "Offsides")?.Value ?? "0"}";
                        sayfa.Cells[satir, 19].Value = $"Faul: {homeStats?.FirstOrDefault(s => s.Type == "Fouls")?.Value ?? "0"}-{awayStats?.FirstOrDefault(s => s.Type == "Fouls")?.Value ?? "0"}";
                        sayfa.Cells[satir, 20].Value = $"Sarı Kart: {homeStats?.FirstOrDefault(s => s.Type == "Yellow Cards")?.Value ?? "0"}-{awayStats?.FirstOrDefault(s => s.Type == "Yellow Cards")?.Value ?? "0"}";
                        sayfa.Cells[satir, 21].Value = $"Kırmızı Kart: {homeStats?.FirstOrDefault(s => s.Type == "Red Cards")?.Value ?? "0"}-{awayStats?.FirstOrDefault(s => s.Type == "Red Cards")?.Value ?? "0"}";
                    }

                    // Kadro bilgilerini al
                    var kadrolar = await _bayTahminService.GetFixtureLineupsAsync(mac.Info.Id);
                    if (kadrolar?.Count > 0)
                    {
                        var homeLineup = kadrolar.FirstOrDefault(s => s.Team?.Id == mac.Teams?.Home?.Id);
                        var awayLineup = kadrolar.FirstOrDefault(s => s.Team?.Id == mac.Teams?.Away?.Id);

                        // Dizilişleri ekle
                        sayfa.Cells[satir, 22].Value = $"{homeLineup?.Formation ?? "-"} - {awayLineup?.Formation ?? "-"}";

                        // İlk 11'leri ekle
                        var homeStartXI = string.Join(", ", homeLineup?.StartXI?.Select(p => p.Player?.Name) ?? Array.Empty<string>());
                        var awayStartXI = string.Join(", ", awayLineup?.StartXI?.Select(p => p.Player?.Name) ?? Array.Empty<string>());
                        sayfa.Cells[satir, 23].Value = $"{homeStartXI} | {awayStartXI}";

                        // Yedekleri ekle
                        var homeSubs = string.Join(", ", homeLineup?.Substitutes?.Select(p => p.Player?.Name) ?? Array.Empty<string>());
                        var awaySubs = string.Join(", ", awayLineup?.Substitutes?.Select(p => p.Player?.Name) ?? Array.Empty<string>());
                        sayfa.Cells[satir, 24].Value = $"{homeSubs} | {awaySubs}";
                    }

                    // Kazanan takımı belirle
                    if (mac.Score?.Fulltime?.Home != null && mac.Score?.Fulltime?.Away != null)
                    {
                        if (mac.Score.Fulltime.Home > mac.Score.Fulltime.Away)
                        {
                            sayfa.Cells[satir, 4].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        }
                        else if (mac.Score.Fulltime.Home < mac.Score.Fulltime.Away)
                        {
                            sayfa.Cells[satir, 5].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        }
                    }

                    // Alternatif satır renklendirmesi
                    if (satir % 2 == 0)
                    {
                        var aralik = sayfa.Cells[satir, 1, satir, 24];
                        aralik.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        aralik.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                    }

                    satir++;
                }

                // Takım isimlerini al
                var homeTeam = maclar.First().Teams?.Home?.Name;
                var awayTeam = maclar.First().Teams?.Away?.Name;

                var stats = maclar
                    .Where(m => m.Score?.Fulltime?.Home != null && m.Score?.Fulltime?.Away != null)
                    .GroupBy(m => true)
                    .Select(g => new
                    {
                        TotalMatches = g.Count(),
                        HomeWins = g.Count(m => m.Score.Fulltime.Home > m.Score.Fulltime.Away),
                        AwayWins = g.Count(m => m.Score.Fulltime.Home < m.Score.Fulltime.Away),
                        Draws = g.Count(m => m.Score.Fulltime.Home == m.Score.Fulltime.Away),
                        HomeGoals = g.Sum(m => m.Score.Fulltime.Home ?? 0),
                        AwayGoals = g.Sum(m => m.Score.Fulltime.Away ?? 0)
                    })
                    .FirstOrDefault();

                // İstatistik özeti ekle
                satir += 2;
                sayfa.Cells[satir, 1].Value = "İstatistik Özeti";
                sayfa.Cells[satir, 1, satir, 24].Merge = true;
                sayfa.Cells[satir, 1].Style.Font.Bold = true;
                sayfa.Cells[satir, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                satir++;
                sayfa.Cells[satir, 1].Value = "Toplam Maç:";
                sayfa.Cells[satir, 2].Value = stats?.TotalMatches ?? 0;
                
                satir++;
                sayfa.Cells[satir, 1].Value = $"{homeTeam} Galibiyetleri:";
                sayfa.Cells[satir, 2].Value = stats?.HomeWins ?? 0;
                
                satir++;
                sayfa.Cells[satir, 1].Value = $"{awayTeam} Galibiyetleri:";
                sayfa.Cells[satir, 2].Value = stats?.AwayWins ?? 0;
                
                satir++;
                sayfa.Cells[satir, 1].Value = "Beraberlikler:";
                sayfa.Cells[satir, 2].Value = stats?.Draws ?? 0;
                
                satir++;
                sayfa.Cells[satir, 1].Value = $"{homeTeam} Golleri:";
                sayfa.Cells[satir, 2].Value = stats?.HomeGoals ?? 0;
                
                satir++;
                sayfa.Cells[satir, 1].Value = $"{awayTeam} Golleri:";
                sayfa.Cells[satir, 2].Value = stats?.AwayGoals ?? 0;

                // Hakem istatistiklerini hesapla
                var hakemStats = maclar
                    .Where(m => !string.IsNullOrEmpty(m.Info?.Referee) && m.Score?.Fulltime?.Home != null && m.Score?.Fulltime?.Away != null)
                    .GroupBy(m => m.Info.Referee)
                    .Select(g => new
                    {
                        Referee = g.Key,
                        TotalMatches = g.Count(),
                        HomeWins = g.Count(m => m.Score.Fulltime.Home > m.Score.Fulltime.Away),
                        AwayWins = g.Count(m => m.Score.Fulltime.Home < m.Score.Fulltime.Away),
                        Draws = g.Count(m => m.Score.Fulltime.Home == m.Score.Fulltime.Away),
                        HomeGoals = g.Sum(m => m.Score.Fulltime.Home ?? 0),
                        AwayGoals = g.Sum(m => m.Score.Fulltime.Away ?? 0),
                        GoalsPerMatch = (double)(g.Sum(m => (m.Score.Fulltime.Home ?? 0) + (m.Score.Fulltime.Away ?? 0))) / g.Count()
                    })
                    .OrderByDescending(h => h.TotalMatches)
                    .ToList();

                // Hakem istatistikleri sayfası
                var hakemSayfa = paket.Workbook.Worksheets.Add("Hakem İstatistikleri");

                // Başlıkları ekle
                hakemSayfa.Cells[1, 1].Value = "Hakem";
                hakemSayfa.Cells[1, 2].Value = "Maç Sayısı";
                hakemSayfa.Cells[1, 3].Value = $"{homeTeam} Galibiyet";
                hakemSayfa.Cells[1, 4].Value = $"{awayTeam} Galibiyet";
                hakemSayfa.Cells[1, 5].Value = "Beraberlik";
                hakemSayfa.Cells[1, 6].Value = $"{homeTeam} Gol";
                hakemSayfa.Cells[1, 7].Value = $"{awayTeam} Gol";
                hakemSayfa.Cells[1, 8].Value = "Maç Başı Gol Ort.";

                // Başlık stilini ayarla
                var hakemBaslikAraligi = hakemSayfa.Cells[1, 1, 1, 8];
                hakemBaslikAraligi.Style.Font.Bold = true;
                hakemBaslikAraligi.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                hakemBaslikAraligi.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Hakem istatistiklerini ekle
                int hakemSatir = 2;
                foreach (var hakem in hakemStats)
                {
                    hakemSayfa.Cells[hakemSatir, 1].Value = hakem.Referee;
                    hakemSayfa.Cells[hakemSatir, 2].Value = hakem.TotalMatches;
                    hakemSayfa.Cells[hakemSatir, 3].Value = hakem.HomeWins;
                    hakemSayfa.Cells[hakemSatir, 4].Value = hakem.AwayWins;
                    hakemSayfa.Cells[hakemSatir, 5].Value = hakem.Draws;
                    hakemSayfa.Cells[hakemSatir, 6].Value = hakem.HomeGoals;
                    hakemSayfa.Cells[hakemSatir, 7].Value = hakem.AwayGoals;
                    hakemSayfa.Cells[hakemSatir, 8].Value = hakem.GoalsPerMatch.ToString("F2");

                    // Alternatif satır renklendirmesi
                    if (hakemSatir % 2 == 0)
                    {
                        var aralik = hakemSayfa.Cells[hakemSatir, 1, hakemSatir, 8];
                        aralik.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        aralik.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                    }

                    hakemSatir++;
                }

                // Hakem sayfası için sütunları otomatik boyutlandır
                hakemSayfa.Cells.AutoFitColumns();

                // Hakem sayfası için border ekle
                var hakemBorderAraligi = hakemSayfa.Cells[1, 1, hakemSatir - 1, 8];
                hakemBorderAraligi.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                hakemBorderAraligi.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                hakemBorderAraligi.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                hakemBorderAraligi.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Hakem sayfası için filtre ekle
                hakemSayfa.Cells[1, 1, 1, 8].AutoFilter = true;

                // Sütunları otomatik boyutlandır
                sayfa.Cells.AutoFitColumns();

                // Tüm hücrelere border ekle
                var veriAraligi = sayfa.Cells[1, 1, satir, 24];
                veriAraligi.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Filtre ekle
                sayfa.Cells[1, 1, 1, 24].AutoFilter = true;

                // Dosya adı oluştur
                string dosyaAdi = $"H2H_{team1Id}-{team2Id}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                _logger.LogInformation("Excel dosyası oluşturuldu. Dosya adı: {FileName}", dosyaAdi);

                // Excel içeriğini byte dizisine çevir
                var icerik = paket.GetAsByteArray();

                // Dosyayı kullanıcıya gönder
                return File(icerik, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", dosyaAdi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Head to head maçlar Excel'e aktarılırken hata oluştu. Takımlar: {Team1Id}-{Team2Id}", team1Id, team2Id);
                return BadRequest($"Head to head maçlar Excel'e aktarılırken hata oluştu: {ex.Message}");
            }
        }
    }
}

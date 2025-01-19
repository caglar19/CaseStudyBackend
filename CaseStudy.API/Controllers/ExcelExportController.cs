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
                sayfa.Cells[1, 6].Value = "Skor (İY)";
                sayfa.Cells[1, 7].Value = "Skor";
                sayfa.Cells[1, 8].Value = "Ev Sahibi Sıra";
                sayfa.Cells[1, 9].Value = "Deplasman Sıra";
                sayfa.Cells[1, 10].Value = "Ev Sahibi Gol İst. (A/Y)";
                sayfa.Cells[1, 11].Value = "Deplasman Gol İst. (A/Y)";
                sayfa.Cells[1, 12].Value = "Stadyum";
                sayfa.Cells[1, 13].Value = "Şehir";
                sayfa.Cells[1, 14].Value = "Hakem";
                sayfa.Cells[1, 15].Value = "Durum";

                // Başlık stilini ayarla
                var baslikAraligi = sayfa.Cells[1, 1, 1, 15];
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
                    sayfa.Cells[satir, 6].Value = mac.Score?.Halftime?.Home != null && mac.Score?.Halftime?.Away != null ? $"{mac.Score.Halftime.Home}-{mac.Score.Halftime.Away}" : "-";
                    sayfa.Cells[satir, 7].Value = mac.Score?.Fulltime?.Home != null && mac.Score?.Fulltime?.Away != null ? $"{mac.Score.Fulltime.Home}-{mac.Score.Fulltime.Away}" : "-";
                    sayfa.Cells[satir, 8].Value = mac.Info?.LeagueStanding?.HomePosition;
                    sayfa.Cells[satir, 9].Value = mac.Info?.LeagueStanding?.AwayPosition;
                    sayfa.Cells[satir, 10].Value = mac.Info?.LeagueStanding != null ? 
                        $"{mac.Info.LeagueStanding.HomeGoalsFor}-{mac.Info.LeagueStanding.HomeGoalsAgainst}" : "-";
                    sayfa.Cells[satir, 11].Value = mac.Info?.LeagueStanding != null ? 
                        $"{mac.Info.LeagueStanding.AwayGoalsFor}-{mac.Info.LeagueStanding.AwayGoalsAgainst}" : "-";
                    sayfa.Cells[satir, 12].Value = mac.Info?.Venue?.Name;
                    sayfa.Cells[satir, 13].Value = mac.Info?.Venue?.City;
                    sayfa.Cells[satir, 14].Value = mac.Info?.Referee;
                    sayfa.Cells[satir, 15].Value = mac.Info?.Status?.Long;

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
                        var aralik = sayfa.Cells[satir, 1, satir, 15];
                        aralik.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        aralik.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                    }

                    satir++;
                }

                // Maç sayısı ve skor ortalaması
                var macSayisi = maclar.Count;
                var toplamGol = maclar
                    .Where(m => m.Score?.Fulltime?.Home != null && m.Score?.Fulltime?.Away != null)
                    .Sum(m => (m.Score.Fulltime.Home ?? 0) + (m.Score.Fulltime.Away ?? 0));
                var golOrtalamasi = macSayisi > 0 ? (double)toplamGol / macSayisi : 0;

                // İlk yarı ve ikinci yarı gol ortalamaları
                var ilkYariGol = maclar
                    .Where(m => m.Score?.Halftime?.Home != null && m.Score?.Halftime?.Away != null)
                    .Sum(m => (m.Score.Halftime.Home ?? 0) + (m.Score.Halftime.Away ?? 0));
                var ilkYariOrtalamasi = macSayisi > 0 ? (double)ilkYariGol / macSayisi : 0;

                var ikinciYariGol = maclar
                    .Where(m => m.Score?.Fulltime?.Home != null && m.Score?.Fulltime?.Away != null &&
                               m.Score?.Halftime?.Home != null && m.Score?.Halftime?.Away != null)
                    .Sum(m => ((m.Score.Fulltime.Home ?? 0) + (m.Score.Fulltime.Away ?? 0)) -
                             ((m.Score.Halftime.Home ?? 0) + (m.Score.Halftime.Away ?? 0)));
                var ikinciYariOrtalamasi = macSayisi > 0 ? (double)ikinciYariGol / macSayisi : 0;

                // Gol dağılımı
                var golDagilimi = maclar
                    .Where(m => m.Score?.Fulltime?.Home != null && m.Score?.Fulltime?.Away != null)
                    .GroupBy(m => (m.Score.Fulltime.Home ?? 0) + (m.Score.Fulltime.Away ?? 0))
                    .OrderBy(g => g.Key)
                    .Select(g => new { ToplamGol = g.Key, MacSayisi = g.Count() })
                    .ToList();

                // İstatistik özeti ekle
                satir += 2;
                sayfa.Cells[satir, 1].Value = "İstatistik Özeti";
                sayfa.Cells[satir, 1, satir, 15].Merge = true;
                sayfa.Cells[satir, 1].Style.Font.Bold = true;
                sayfa.Cells[satir, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                
                satir++;
                sayfa.Cells[satir, 1].Value = "Toplam Maç:";
                sayfa.Cells[satir, 2].Value = macSayisi;
                
                satir++;
                sayfa.Cells[satir, 1].Value = "Toplam Gol:";
                sayfa.Cells[satir, 2].Value = toplamGol;
                
                satir++;
                sayfa.Cells[satir, 1].Value = "Gol Ortalaması:";
                sayfa.Cells[satir, 2].Value = golOrtalamasi.ToString("F2");
                
                satir++;
                sayfa.Cells[satir, 1].Value = "İlk Yarı Gol Ortalaması:";
                sayfa.Cells[satir, 2].Value = ilkYariOrtalamasi.ToString("F2");
                
                satir++;
                sayfa.Cells[satir, 1].Value = "İkinci Yarı Gol Ortalaması:";
                sayfa.Cells[satir, 2].Value = ikinciYariOrtalamasi.ToString("F2");

                // Gol dağılımı sayfası
                var golDagilimiSayfa = paket.Workbook.Worksheets.Add("Gol Dağılımı");

                // Başlıkları ekle
                golDagilimiSayfa.Cells[1, 1].Value = "Toplam Gol";
                golDagilimiSayfa.Cells[1, 2].Value = "Maç Sayısı";

                // Başlık stilini ayarla
                var golDagilimiBaslikAraligi = golDagilimiSayfa.Cells[1, 1, 1, 2];
                golDagilimiBaslikAraligi.Style.Font.Bold = true;
                golDagilimiBaslikAraligi.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                golDagilimiBaslikAraligi.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Gol dağılımı verilerini ekle
                int golDagilimiSatir = 2;
                foreach (var gol in golDagilimi)
                {
                    golDagilimiSayfa.Cells[golDagilimiSatir, 1].Value = gol.ToplamGol;
                    golDagilimiSayfa.Cells[golDagilimiSatir, 2].Value = gol.MacSayisi;

                    // Alternatif satır renklendirmesi
                    if (golDagilimiSatir % 2 == 0)
                    {
                        var aralik = golDagilimiSayfa.Cells[golDagilimiSatir, 1, golDagilimiSatir, 2];
                        aralik.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        aralik.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                    }

                    golDagilimiSatir++;
                }

                // Gol dağılımı sayfası için sütunları otomatik boyutlandır
                golDagilimiSayfa.Cells.AutoFitColumns();

                // Gol dağılımı sayfası için border ekle
                var golDagilimiVeriAraligi = golDagilimiSayfa.Cells[1, 1, golDagilimiSatir - 1, 2];
                golDagilimiVeriAraligi.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                golDagilimiVeriAraligi.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                golDagilimiVeriAraligi.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                golDagilimiVeriAraligi.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Gol dağılımı sayfası için filtre ekle
                golDagilimiSayfa.Cells[1, 1, 1, 2].AutoFilter = true;

                // Sütunları otomatik boyutlandır
                sayfa.Cells.AutoFitColumns();

                // Tüm hücrelere border ekle
                var veriAraligi = sayfa.Cells[1, 1, satir, 15];
                veriAraligi.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Filtre ekle
                sayfa.Cells[1, 1, 1, 15].AutoFilter = true;

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

                // Başlık stilini ayarla
                var baslikAraligi = sayfa.Cells[1, 1, 1, 13];
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
                        var aralik = sayfa.Cells[satir, 1, satir, 13];
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
                sayfa.Cells[satir, 1, satir, 13].Merge = true;
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
                var veriAraligi = sayfa.Cells[1, 1, satir, 13];
                veriAraligi.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                veriAraligi.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Filtre ekle
                sayfa.Cells[1, 1, 1, 13].AutoFilter = true;

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

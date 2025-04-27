using System;
using System.Collections.Generic;
using System.Linq;
using CaseStudy.Application.Models.Roulette;

namespace CaseStudy.Application.Strategies
{
    public class SectorBasedAnalysisStrategy : IPredictionStrategy
    {
        private readonly Random _random;
        
        // Rulet çarkındaki fiziksel sektörler
        private readonly int[] _wheelSequence = new int[] {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };
        
        // Çark üzerinde tanımlanan sektörler (komşu sayı grupları)
        private readonly List<int[]> _sectors;
        
        public string Name => "Sektör Bazlı Analiz";
        
        public SectorBasedAnalysisStrategy()
        {
            _random = new Random();
            
            // Çark üzerinde 12'şer sayıdan oluşan 3 sektör tanımla
            _sectors = new List<int[]>();
            
            // Sektör 1: 0'dan başlayıp 12 sayı
            var sector1 = new int[12];
            Array.Copy(_wheelSequence, 0, sector1, 0, 12);
            _sectors.Add(sector1);
            
            // Sektör 2: 12'den başlayıp 12 sayı
            var sector2 = new int[12];
            Array.Copy(_wheelSequence, 12, sector2, 0, 12);
            _sectors.Add(sector2);
            
            // Sektör 3: 24'ten başlayıp 12 sayı (ya da kalan sayılar)
            var sector3 = new int[_wheelSequence.Length - 24];
            Array.Copy(_wheelSequence, 24, sector3, 0, _wheelSequence.Length - 24);
            _sectors.Add(sector3);
        }
        
        public int PredictNextNumber(List<int> numbers)
        {
            if (numbers == null || numbers.Count < 10)
            {
                return _random.Next(0, 37);
            }
            
            // Son 20 sayıyı al
            var recentNumbers = numbers.Take(20).ToList();
            
            // Sektör analizine göre sayıları grupla
            var sectorHits = AnalyzeSectorHits(recentNumbers);
            
            // En çok isabet alan sektörü bul
            var mostHitSector = sectorHits.OrderByDescending(kv => kv.Value).First().Key;
            
            // Sektördeki sayılardan birini rastgele seç (ağırlıklı olarak)
            var sectorNumbers = _sectors[mostHitSector];
            
            // Fiziksel olarak yakın sayıların birbirini takip etme eğilimini analiz et
            var lastNumber = numbers.First();
            var lastNumberIndex = Array.IndexOf(_wheelSequence, lastNumber);
            
            // Son çıkan sayının etrafındaki 5 sayıyı potansiyel adaylar olarak değerlendir
            var potentialNextNumbers = new List<int>();
            var radius = 5;
            
            for (int i = -radius; i <= radius; i++)
            {
                int index = (lastNumberIndex + i + _wheelSequence.Length) % _wheelSequence.Length;
                potentialNextNumbers.Add(_wheelSequence[index]);
            }
            
            // Sektördeki sayılar ile potansiyel sayıları karşılaştır
            var candidates = sectorNumbers.Intersect(potentialNextNumbers).ToList();
            
            // Eğer kesişen sayı yoksa, en aktif sektörden rastgele seç
            if (candidates.Count == 0)
            {
                return sectorNumbers[_random.Next(sectorNumbers.Length)];
            }
            
            // Kesişen sayılardan rastgele birini seç
            return candidates[_random.Next(candidates.Count)];
        }
        
        public bool CheckPredictionAccuracy(int predictedNumber, int actualNumber, int[] neighbors)
        {
            return predictedNumber == actualNumber;
        }
        
        private Dictionary<int, int> AnalyzeSectorHits(List<int> numbers)
        {
            var hits = new Dictionary<int, int>
            {
                { 0, 0 }, // Sektör 1
                { 1, 0 }, // Sektör 2
                { 2, 0 }  // Sektör 3
            };
            
            foreach (var num in numbers)
            {
                for (int i = 0; i < _sectors.Count; i++)
                {
                    if (_sectors[i].Contains(num))
                    {
                        hits[i]++;
                        break;
                    }
                }
            }
            
            return hits;
        }
    }
}

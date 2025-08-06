using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JidamVision4.Util
{
    public class ImageFileSorter
    {
        public static List<string> GetSortedImages(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"폴더를 찾을 수 없습니다: {folderPath}");

            // 지원하는 이미지 확장자 목록
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" };

            // 폴더 내 이미지 파일 필터링
            var imageFiles = Directory.GetFiles(folderPath)
                                      .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                                      .ToList();

            // 자연 정렬을 적용한 정렬
            imageFiles.Sort(CompareFileNames);

            return imageFiles;
        }

        private static int CompareFileNames(string a, string b)
        {
            return NaturalSortComparer(a, b);
        }

        private static int NaturalSortComparer(string s1, string s2)
        {
            var regex = new Regex(@"\d+|\D+");
            var parts1 = regex.Matches(Path.GetFileNameWithoutExtension(s1)).Cast<Match>().Select(m => m.Value).ToArray();
            var parts2 = regex.Matches(Path.GetFileNameWithoutExtension(s2)).Cast<Match>().Select(m => m.Value).ToArray();

            int minLen = Math.Min(parts1.Length, parts2.Length);

            for (int i = 0; i < minLen; i++)
            {
                if (int.TryParse(parts1[i], out int num1) && int.TryParse(parts2[i], out int num2))
                {
                    int result = num1.CompareTo(num2);
                    if (result != 0) return result;
                }
                else
                {
                    int result = string.Compare(parts1[i], parts2[i], StringComparison.OrdinalIgnoreCase);
                    if (result != 0) return result;
                }
            }

            return parts1.Length.CompareTo(parts2.Length);
        }
    }
}

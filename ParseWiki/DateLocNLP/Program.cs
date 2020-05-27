using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProtoNLP
{
    static class Program
    {
        
        
        static async Task Main(string[] args)
        {
            var sampleText =
                "MacArthur entered West Point on 13 June 1899, and his mother also moved there, to a suite at Craney's Hotel, which overlooked the grounds of the Academy.";
//             sampleText = @"General of the Army Douglas MacArthur (26 January 1880 – 5 April 1964) was an American five-star general and Field Marshal of the Philippine Army. He was Chief of Staff of the United States Army during the 1930s and played a prominent role in the Pacific theater during World War II. He received the Medal of Honor for his service in the Philippines Campaign, which made him and his father Arthur MacArthur Jr. the first father and son to be awarded the medal. He was one of only five to rise to the rank of General of the Army in the US Army, and the only one conferred the rank of field marshal in the Philippine Army.
//
// Raised in a military family in the American Old West, MacArthur was valedictorian at the West Texas Military Academy where he finished high school, and First Captain at the United States Military Academy at West Point, where he graduated top of the class of 1903. During the 1914 United States occupation of Veracruz, he conducted a reconnaissance mission, for which he was nominated for the Medal of Honor. In 1917, he was promoted from major to colonel and became chief of staff of the 42nd (Rainbow) Division. In the fighting on the Western Front during World War I, he rose to the rank of brigadier general, was again nominated for a Medal of Honor, and was awarded the Distinguished Service Cross twice and the Silver Star seven times.
//
// From 1919 to 1922, MacArthur served as Superintendent of the U.S. Military Academy at West Point, where he attempted a series of reforms. His next assignment was in the Philippines, where in 1924 he was instrumental in quelling the Philippine Scout Mutiny. In 1925, he became the Army's youngest major general. He served on the court-martial of Brigadier General Billy Mitchell and was president of the American Olympic Committee during the 1928 Summer Olympics in Amsterdam. In 1930, he became Chief of Staff of the United States Army. As such, he was involved in the expulsion of the Bonus Army protesters from Washington, D.C. in 1932, and the establishment and organization of the Civilian Conservation Corps. He retired from the US Army in 1937 to become Military Advisor to the Commonwealth Government of the Philippines.
//
// MacArthur was recalled to active duty in 1941 as commander of United States Army Forces in the Far East. A series of disasters followed, starting with the destruction of his air forces on 8 December 1941 and the Japanese invasion of the Philippines. MacArthur's forces were soon compelled to withdraw to Bataan, where they held out until May 1942. In March 1942, MacArthur, his family and his staff left nearby Corregidor Island in PT boats and escaped to Australia, where MacArthur became Supreme Commander, Southwest Pacific Area. Upon his arrival, MacArthur gave a speech in which he famously promised ""I shall return""
//                  to the Philippines. After more than two years of fighting in the Pacific, he fulfilled that promise. For his defense of the Philippines, MacArthur was awarded the Medal of Honor. He officially accepted the Surrender of Japan on 2 September 1945 aboard the USS Missouri, which was anchored in Tokyo Bay, and he oversaw the occupation of Japan from 1945 to 1951. As the effective ruler of Japan, he oversaw sweeping economic, political and social changes. He led the United Nations Command in the Korean War with initial success; however, the controversial invasion of North Korea provoked Chinese intervention, and a series of major defeats. MacArthur was contentiously removed from command by President Harry S. Truman on 11 April 1951. He later became chairman of the board of Remington Rand. ";
            sampleText =
                "In August 1907, MacArthur was sent to the engineer district office in Milwaukee, where his parents were living. In April 1908, he was posted to Fort Leavenworth, where he was given his first command, Company K, 3rd Engineer Battalion.[28] He became battalion adjutant in 1909 and then engineer officer at Fort Leavenworth in 1910. MacArthur was promoted to captain in February 1911 and was appointed as head of the Military Engineering Department and the Field Engineer School. He participated in exercises at San Antonio, Texas, with the Maneuver Division in 1911 and served in Panama on detached duty in January and February 1912. The sudden death of their father on 5 September 1912 brought Douglas and his brother Arthur back to Milwaukee to care for their mother, whose health had deteriorated. MacArthur requested a transfer to Washington, D.C. so his mother could be near Johns Hopkins Hospital. Army Chief of Staff, Major General Leonard Wood, took up the matter with Secretary of War Henry L. Stimson, who arranged for MacArthur to be posted to the Office of the Chief of Staff in 1912.[29] ";
            sampleText =
                "The political group that proved most troublesome for Kerensky, and would eventually overthrow him, was the Bolshevik Party, led by Vladimir Lenin. Lenin had been living in exile in neutral Switzerland and, due to democratization of politics after the February Revolution, which legalized formerly banned political parties, he perceived the opportunity for his Marxist revolution. Although return to Russia had become a possibility, the war made it logistically difficult. Eventually, German officials arranged for Lenin to pass through their territory, hoping that his activities would weaken Russia or even – if the Bolsheviks came to power – lead to Russia's withdrawal from the war. Lenin and his associates, however, had to agree to travel to Russia in a sealed train: Germany would not take the chance that he would foment revolution in Germany. After passing through the front, he arrived in Petrograd in April 1917. ";
            var nlpBaseAddress = "http://localhost:9000";
            var options = new NlpProperties();
            // options.annotators = "tokenize, ssplit, pos, lemma, ner, parse, coref";
            // options.annotators = "tokenize, ssplit, pos, ner, coref";
            options.annotators = "openie, coref";
            options.outputformat = "text";
            var jsonOptions = JsonSerializer.Serialize(options);
            var qstringProperties = new Dictionary<string,string>();
            qstringProperties.Add("properties", jsonOptions);
            var qString = ToQueryString(qstringProperties);
            var urlPlusQuery = nlpBaseAddress + qString;
            var content = new StringContent(sampleText);
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            var client = new HttpClient();
            var response = await client.PostAsync(urlPlusQuery, content);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApplicationException("Subject-Object tuple extraction returned an unexpected response from the subject-object service");
            }
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<NlpResult>(jsonResult);

            await System.IO.File.WriteAllTextAsync("/home/david/output.json", jsonResult);
            // Console.Out.WriteLine(jsonResult);
        }
        
        private static string ToQueryString(Dictionary<string,string> args)
        {
            var sb = new StringBuilder("?");
            bool first = true;
            foreach (var item in args)
            {
                if (!first)
                {
                    sb.Append("&");
                }
                sb.AppendFormat("{0}={1}", Uri.EscapeDataString(item.Key), Uri.EscapeDataString(item.Value));
                first = false;
            }
            return sb.ToString();
        }
    }
}
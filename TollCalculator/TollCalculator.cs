using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TollCalculator.Vehicles;

namespace TollCalculator
{
    public static class TollCalculator
    {

        /// <summary>Calculate the total toll fee</summary>
        /// <param name="vehicle">the vehicle</param> 
        /// <param name="dates">date and time of all passes</param>
        /// <returns> the total toll fee</returns>
        public static int GetTollFee(IVehicle vehicle, DateTime[] dates)
        {
            if (vehicle.TollFree()) 
                return 0;
            
            Array.Sort(dates);

            var intervalStart = dates[0];
            var totalFeeDay = 0;
            var highestFee = 0;
            var totalFee = 0;

            foreach (var date in dates)
            {
                if (date.Date != intervalStart.Date)
                {
                    totalFee += totalFeeDay;
                    totalFeeDay = 0;
                }
                
                var fee = GetTollFee(date);

                if (date < intervalStart.AddHours(1))
                {
                    if (fee > highestFee)
                        highestFee = fee;
                }
                else
                {
                    intervalStart = date;
                    totalFeeDay += highestFee + fee;

                    if (totalFeeDay > 60)
                        totalFeeDay = 60;

                    highestFee = 0;
                }
            }

            if (highestFee > 0)
                totalFeeDay = highestFee;

            return totalFee + totalFeeDay;
        }
        
        private static int GetTollFee(DateTime date)
        {
            if (IsTollFreeDate(date)) 
                return 0;

            if (TimeBetween(date, (6, 0), (6,29))) return 8;
            if (TimeBetween(date, (6, 30), (6, 59))) return 13;
            if (TimeBetween(date, (7, 0), (7, 59))) return 18;
            if (TimeBetween(date, (8, 0), (8, 29))) return 13;
            if (TimeBetween(date, (8, 30), (14, 59))) return 8;
            if (TimeBetween(date, (15, 00), (15, 29))) return 13;
            if (TimeBetween(date, (15, 30), (16, 59))) return 18;
            if (TimeBetween(date, (17, 00), (17, 59))) return 13;
            if (TimeBetween(date, (18, 00), (18, 29))) return 8;

            return 0;
            
            static bool TimeBetween(DateTime passageTime, (int Hour, int Minute) startTime, (int Hour, int Minute) endTime)
            {
                var now = passageTime.TimeOfDay;
                var start = new TimeSpan(startTime.Hour, startTime.Minute, 0);
                var end = new TimeSpan(endTime.Hour, endTime.Minute, 59);
                
                return (now >= start) && (now <= end);
            }
        }

        private static bool IsTollFreeDate(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || date.Month == 7) 
                return true;

            //TODO:Borde hämtas från config-fil eller liknade och borde inte användas i något produktionssystem :)
            var client = new HttpClient(){BaseAddress = new Uri("https://svenskahelgdagar.info/v2/")};
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var accessToken = GetAccessToken(client).Result;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",accessToken);

            return IsPublicSunday(date, client).Result || IsPublicSunday(date.AddDays(1), client).Result;

            static async Task<string> GetAccessToken(HttpClient client)
            {
                //TODO: Borde hämtas från någon config-fil eller liknade. 
                var clientId = "linusnoren1834";
                var clientSecret = "a2fe67-717c47-ca11dd-ef8571-850fe6";

                var postData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                };

                var content = new FormUrlEncodedContent(postData);

                var response = await client.PostAsync("access_token", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JsonConvert.DeserializeObject(jsonString);
                    return responseData?.access_token;
                }

                throw new HttpRequestException(response.StatusCode.ToString());
            }

            static async Task<bool> IsPublicSunday(DateTime dateTime, HttpClient httpClient)
            {

                var response = await httpClient.GetAsync($"date/{dateTime.ToShortDateString()}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JsonConvert.DeserializeObject(jsonString);

                    //Om det inte är helgdag är response en tom JArray...
                    return responseData?.response is JObject && (bool) responseData.response.public_sunday;
                }
                    
                throw new HttpRequestException(response.StatusCode.ToString());

            }
        }
    }
}
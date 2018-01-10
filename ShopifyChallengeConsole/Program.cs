/* Shopify Software Engineering Code Challenge
 * 
 *      Created By : Rawa Jalal
 *      Date created: January 10th, 2018 
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShopifyChallengeConsole
{
    /// <summary>
    /// Plain CLR object representing the array of menus
    /// </summary>
    public class Menus
    {
        public Menu[] menus { get; set; }
    }

    /// <summary>
    /// Plain CLR object representing the menu properties
    /// </summary>
    public class Menu
    {
        public int id { get; set; }
        public string data { get; set; }
        public int? parent_id { get; set; }
        public int[] child_ids { get; set; }
    }


    class Program
    {
        static HttpClient client = new HttpClient();

        static string apiUrl =
            "https://backend-challenge-summer-2018.herokuapp.com/challenges.json?id=1&page=1";

        static List<Menu> invalidMenus = new List<Menu>();
        static List<Menu> validMenus = new List<Menu>();

        /// <summary>
        /// Gets the menu JSON object by making a request to API.
        /// </summary>
        /// <param name="path">The API url path.</param>
        /// <returns>A plain CLR Menus object.</returns>
        static async Task<Menus> GetMenuAsync(string path)
        {
            Menus menu = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                menu = await response.Content.ReadAsAsync<Menus>();
            }
            return menu;
        }

        /// <summary>
        /// Lookup function to find children menu based on id.
        /// </summary>
        /// <param name="menus">The collection of menus to search through.</param>
        /// <param name="id">The id of the child menu to find.</param>
        /// <returns>A Menu CLR object.</returns>
        static Menu GetChildMenu(Menus menus, int id)
        {
            for (int i = 0; i < menus.menus.Length; i++)
            {
                if (menus.menus[i].id == id)
                {
                    return menus.menus[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Main task, finds the invalid and valid menus.
        /// </summary>
        /// <returns></returns>
        static async Task RunAsync()
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var menus =
                await GetMenuAsync(apiUrl);
            FindCyclicalReferences(menus);

            var unserializedValidMenus = new { valid_menus = new List<Menu>() };
            foreach (var menu in validMenus)
            {
                unserializedValidMenus.valid_menus.Add(menu);
            }
            string serializedValidMenus = JsonConvert.SerializeObject(unserializedValidMenus);


            var unserializedInvalidMenus = new { invalid_menus = new List<Menu>() };
            foreach (var menu in invalidMenus)
            {
                unserializedInvalidMenus.invalid_menus.Add(menu);
            }
            string serializedInvalidMenus = JsonConvert.SerializeObject(unserializedInvalidMenus);

            Console.WriteLine(serializedValidMenus);
            Console.WriteLine(serializedInvalidMenus);

        }

        /// <summary>
        /// Determines which menus have cyclical references.
        /// </summary>
        /// <param name="menus">The Menu collection to search through.</param>
        private static void FindCyclicalReferences(Menus menus)
        {
            for (int i = 0; i < menus.menus.Length; i++)
            {
                Menu rootMenu = menus.menus[i];
                Queue<int> childIds = new Queue<int>();
                foreach (int id in menus.menus[i].child_ids)
                {
                    childIds.Enqueue(id);
                }

                while (childIds.Count > 0)
                {
                    Menu childMenu = GetChildMenu(menus, childIds.Dequeue());
                    if (childMenu == null) break;
                    foreach (int childId in childMenu.child_ids)
                    {
                        if (childId == rootMenu.id)
                        {
                            invalidMenus.Add(rootMenu);
                        }
                    }

                }
                validMenus.Add(rootMenu);
            }
        }

        /// <summary>
        /// Main program entry.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
            Console.ReadLine();
        }
       
    }
}

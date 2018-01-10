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
    /// Plain CLR object representing the array of menus.
    /// </summary>
    public class Menus
    {
        public Menus()
        {
            menus = new List<Menu>();
        }
        public List<Menu> menus { get; set; }
        public Pagination pagination { get; set; }
    }

    /// <summary>
    /// Plain CLR object representing the pagination information.
    /// </summary>
    public class Pagination
    {
        public int current_page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
    }

    /// <summary>
    /// Plain CLR object representing the menu properties.
    /// </summary>
    public class Menu
    {
        public int id { get; set; }
        public string data { get; set; }
        public int? parent_id { get; set; }
        public int[] child_ids { get; set; }
    }

    public class OutputMenu
    {
        public int root_id { get; set; }
        public int[] children { get; set; }
    }


    class Program
    {
        static HttpClient client = new HttpClient();

        static string apiUrl =
            "https://backend-challenge-summer-2018.herokuapp.com/challenges.json?id=1&page=1";

        static List<Menu> _invalidMenus = new List<Menu>();
        static List<Menu> _validMenus = new List<Menu>();
        static List<int> _discoveredMenus = new List<int>();

        /// <summary>
        /// Updates the api page url.
        /// </summary>
        /// <param name="page">The page to update the url to.</param>
        /// <returns>The new API url.</returns>
        static string UpdateApiUrlPage(int page)
        {
            return apiUrl.Substring(0, apiUrl.Length - 1) + page;
        }

        /// <summary>
        /// Checks for duplicates in a list of integers.
        /// </summary>
        /// <param name="valuesToCheck">The list of integers to check for duplicates.</param>
        /// <returns>True if duplicates exist.</returns>
        static bool DuplicatesFound(List<int> valuesToCheck)
        {
            List<int> newList = new List<int>();
            foreach (int i in valuesToCheck)
            {
                if (!newList.Contains(i))
                {
                    newList.Add(i);
                }
            }
            return valuesToCheck.Count != newList.Count;
        }

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
            for (int i = 0; i < menus.menus.Count; i++)
            {
                if (menus.menus[i].id == id)
                {
                    return menus.menus[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all of the children menus for a particular menu.
        /// </summary>
        /// <param name="menus">The collection of menus.</param>
        /// <param name="menu">The parent menu of the children.</param>
        /// <returns>A list of children Menu.</returns>
        static List<Menu> GetChildMenusForThisMenu(Menus menus,Menu menu)
        {
            List<Menu> childMenus = new List<Menu>();
            foreach (var childId in menu.child_ids)
            {
                childMenus.Add(GetChildMenu(menus, childId));
            }
            return childMenus;
        }

        /// <summary>
        /// Gets the number of pages for menus.
        /// </summary>
        /// <returns>The number of pages.</returns>
        private static async Task<int> GetNumberOfPages()
        {
            Menus tempMenus =
                await GetMenuAsync(apiUrl);
            int numberOfPages =
                (int)Math.Ceiling((double)tempMenus.pagination.total / tempMenus.pagination.per_page);
            return numberOfPages;
        }

        /// <summary>
        /// Determines which menus have cyclical references.
        /// </summary>
        /// <param name="menus">The Menu collection to search through.</param>
        private static void FindCyclicalReferences(Menus menus)
        {
            for (int i = 0; i < menus.menus.Count; i++)
            {
                Menu rootMenu = menus.menus[i];
                DiscoverNodes(menus, rootMenu);
                if (DuplicatesFound(_discoveredMenus))
                {
                    _invalidMenus.Add(rootMenu);
                }
                else
                {
                    _validMenus.Add(rootMenu);
                }
                _discoveredMenus = new List<int>();
            }
        }

        /// <summary>
        /// Recursive function that traverses the menu tree.
        /// </summary>
        /// <param name="menus">The collection of menus.</param>
        /// <param name="menu">The menu being traversed.</param>
        static void DiscoverNodes(Menus menus, Menu menu)
        {
            _discoveredMenus.Add(menu.id);
            foreach (var childMenu in GetChildMenusForThisMenu(menus, menu))
            {
                if (!_discoveredMenus.Contains(childMenu.id))
                {
                    DiscoverNodes(menus, childMenu);
                }
                else
                {
                    _discoveredMenus.Add(childMenu.id);
                }
            }

        }

        /// <summary>
        /// Main task, finds the invalid and valid menus and outputs the final result.
        /// </summary>
        static async Task RunAsync()
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            int numberOfPages = await GetNumberOfPages();

            Menus combinedMenus = new Menus();
            for (int i = 0; i < numberOfPages; i++)
            {
                Menus menus = await GetMenuAsync(UpdateApiUrlPage(i + 1));
                foreach (var menu in menus.menus)
                {
                    combinedMenus.menus.Add(menu);
                }    
            }

            FindCyclicalReferences(combinedMenus);


            var unserializedValidMenus = new { valid_menus = new List<OutputMenu>() };
            foreach (var menu in _validMenus)
            {
                unserializedValidMenus.valid_menus.Add(new OutputMenu
                {
                    root_id = menu.id,
                    children = menu.child_ids
                });
            }
            string serializedValidMenus = JsonConvert.SerializeObject(unserializedValidMenus);


            var unserializedInvalidMenus = new { invalid_menus = new List<OutputMenu>() };
            foreach (var menu in _invalidMenus)
            {
                unserializedInvalidMenus.invalid_menus.Add(new OutputMenu
                {
                    root_id = menu.id,
                    children = menu.child_ids
                });
            }
            string serializedInvalidMenus = JsonConvert.SerializeObject(unserializedInvalidMenus);

            Console.WriteLine(serializedValidMenus);
            Console.WriteLine(serializedInvalidMenus);

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ShopifyChallengeConsole
{
    public class Menus
    {
        public Menu[] menus { get; set; }
    }

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

        static async Task<string> GetMenuAsStringAsync(string path)
        {
            string menu = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                menu = await response.Content.ReadAsStringAsync();
            }
            return menu;
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("https://backend-challenge-summer-2018.herokuapp.com/challenges.json?id=1&page=1");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var menus =
                await GetMenuAsync("https://backend-challenge-summer-2018.herokuapp.com/challenges.json?id=1&page=1");

            string menuAsStrings =
                await GetMenuAsStringAsync(
                    "https://backend-challenge-summer-2018.herokuapp.com/challenges.json?id=1&page=1");

            Console.WriteLine(menuAsStrings);

            List<Menu> invalidMenus = new List<Menu>();
            List<Menu> validMenus = new List<Menu>();

            for (int i = 0; i < menus.menus.Length; i++)
            {
                Menu rootMenu = menus.menus[i];
                Queue<int> childIds = new Queue<int>();
                foreach (int id in menus.menus[i].child_ids)
                {
                    childIds.Enqueue(id);
                }
                
                while (childIds.Count>0)
                {
                    Menu childMenu = GetChildMenu(menus,childIds.Dequeue());
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

            foreach (var menu in validMenus)
            {
                Console.WriteLine(menu.id);
            }
            
        }


        static Menu GetChildMenu(Menus menus,int id)
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

        static void Main(string[] args)
        {

            RunAsync().GetAwaiter().GetResult();
            Console.ReadLine();
        }

       
    }
}

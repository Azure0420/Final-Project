//Christian Reyniel C. Mangas
//CPE261 - H3
//Final Project - Local Farmer's Product System
using BetterConsoleTables;
using Newtonsoft.Json;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web.Security;
using System.Xml.Linq;


namespace FinalProject
{
    public abstract class User
    {
        public int userID { get; set; }

        protected string password { get; set; }
        protected string name { get; set; }
        protected string contactInfo { get; set; }
        protected string address { get; set; }


        public abstract void Register();
        public abstract void UpdateInfo();
    }

    public class Farmer : User
    {
        bool repeat = true;

        public void AddProduct()
        {
            bool repeat = true;
            string productFile = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers\{userID}\{userID}_Product.txt";
            Random rand = new Random();
            int productID = rand.Next(10000, 100000);
            string category = null;


            try
            {
                Console.Clear();
                var consumerTable = new Table("Option", "Category");
                consumerTable.AddRow("1", "Vegan")
                             .AddRow("2", "Non-vegan")
                             .AddRow("3", "Dairy")
                             .AddRow("4", "Meat")
                             .AddRow("5", "Fruits")
                             .AddRow("6", "Vegetables");
                Console.WriteLine(consumerTable.ToString());
                Console.Write("Choice: ");
                int subchoice = int.Parse(Console.ReadLine());
                Console.WriteLine();
                Console.Clear();
                switch (subchoice)
                {
                    case 1:
                        category = "Vegan";
                        break;
                    case 2:
                        category = "Non-Vegan";
                        break;
                    case 3:
                        category = "Dairy";
                        break;
                    case 4:
                        category = "Meat";
                        break;
                    case 5:
                        category = "Fruits";
                        break;
                    case 6:
                        category = "Vegetables";
                        break;
                    default:
                        Console.WriteLine("Invalid Choice.");
                        break;
                }
                Console.Write("Product name: ");
                string productName = Console.ReadLine();

                Console.Write("Product quantity(in Kilograms): ");
                string productQuantity = Console.ReadLine();

                Console.Write("Product price(per Kilograms): ");
                double productPrice = double.Parse(Console.ReadLine());

                Console.WriteLine($"Date Harvested (mm/dd/{DateTime.Now.Year})");
                Console.Write("Month: ");
                int month = int.Parse(Console.ReadLine());

                Console.Write("Day: ");
                int day = int.Parse(Console.ReadLine());



                Console.WriteLine("Expiry date (mm/dd/yyyy)");
                Console.Write("Month: ");
                int exp_month = int.Parse(Console.ReadLine());

                Console.Write("Day: ");
                int exp_day = int.Parse(Console.ReadLine());

                Console.Write("Year: ");
                int exp_year = int.Parse(Console.ReadLine());

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Product has been added!");
                Console.ResetColor();
                Console.WriteLine($"Product ID: {productID}");
                Console.WriteLine();


                using (StreamWriter writer = File.AppendText(productFile))//displays listing and information of product
                {
                    writer.WriteLine($"{productName} | {productQuantity} kg | {productPrice} per kg | {category} | {productID} | {month}-{day}-{DateTime.Now.Year} | {exp_month}-{exp_day}-{exp_year} |");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Invalid input");
            }

        }

        public void removeProduct()
        {
            bool repeat = true;
            string productFile = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers\{userID}\{userID}_Product.txt";
            Console.Write("Enter ID product: ");
            int productID = int.Parse(Console.ReadLine());
            //finds specific product based on product ID
            string[] product = File.ReadAllLines(productFile);
            for (int i = 0; i < product.Length; i++)
            {
                if (product[i].Contains(productID.ToString()))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Product found!");
                    Console.ResetColor();
                    Console.WriteLine();
                    var deductProduct = new Table("Product", "Quantity(in Kg)", "Price(per Kg)", "Category", "Product ID", "Day Harvested", "Expiry Date");
                    string[] parts = product[i].Split('|');
                    string quantityText = parts[1].Trim();
                    double quantity = double.Parse(quantityText.Replace("kg", "").Trim());
                    if (parts.Length >= 7)
                    {
                        deductProduct.AddRow(
                            parts[0],
                            parts[1],
                            parts[2],
                            parts[3],
                            parts[4],
                            parts[5],
                            parts[6]);
                    }

                    Console.WriteLine(deductProduct);

                    Console.Write("Deduct amount (kg): ");//deducts the amount of quantity
                    double deductAmount = double.Parse(Console.ReadLine());

                    if (deductAmount < quantity)
                    {
                        quantity -= deductAmount;
                    }
                    else if (deductAmount > quantity)
                    {
                        Console.WriteLine("Stock out of range");
                        break;
                    }
                    else if (deductAmount == quantity)
                    {
                        quantity = 0;
                    }

                    //changes to "out of stock"
                    if (quantity > 0)
                    {
                        parts[1] = $" {quantity} kg ";
                    }
                    else if (quantity == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        parts[1] = " Sold Out ";
                        Console.ResetColor();
                    }
                    product[i] = string.Join("|", parts);

                    // update the file
                    File.WriteAllLines(productFile, product);

                    //extract the price from the file
                    string priceText = parts[2].Trim();
                    double price = double.Parse(priceText.Replace("per kg", "").Trim());

                    transactionHistory(productID, deductAmount * price);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Deducted {deductAmount} kg. Remaining quantity: {quantity} kg.");
                    Console.ResetColor();
                    repeat = false;
                    break;
                }
            }
        }

        public void DisplayProduct()
        {
            string productFile = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers\{userID}\{userID}_Product.txt";
            if (File.Exists(productFile))
            {
                string[] lines = File.ReadAllLines(productFile);
                if (lines.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No products found.");
                    Console.ResetColor();
                    return;
                }

                var productTable = new Table("Product Name", "Quantity (in Kg)", "Price (per Kg)", "Category", "Product ID", "Harvest Date", "Expiry Date");

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] displayInfo = lines[i].Split('|');
                    if (displayInfo.Length >= 7)
                    {
                        productTable.AddRow(
                             displayInfo[0].Trim(),
                             displayInfo[1].Trim(),
                             displayInfo[2].Trim(),
                             displayInfo[3].Trim(),
                             displayInfo[4].Trim(),
                             displayInfo[5].Trim(),
                             displayInfo[6].Trim()
                             );

                    }
                }

                Console.WriteLine(productTable);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Press Enter to continue");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: File not found.");
                Console.ResetColor();
            }

        }

        public void transactionHistory(int productID, double moneyEarned)
        {
            string transactionFile = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers\{userID}\transactionHistory.txt";
            Random rand = new Random();
            int transactionID = rand.Next(10000, 100000);
            string date = DateTime.Now.ToString("MM/dd/yyyy HH:mm");

            using (StreamWriter writer = File.AppendText(transactionFile))
            {
                writer.WriteLine($"{transactionID} | {productID} | {date} | {moneyEarned}");
            }
        }

        public void viewTransactionHistory()
        {
            Console.Clear();
            string transactionFile = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers\{userID}\transactionHistory.txt";
            if (File.Exists(transactionFile))
            {
                string[] lines = File.ReadAllLines(transactionFile);
                if (lines.Length == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No transaction history.");
                    Console.ResetColor();
                }
                var transactionTable = new Table("Transaction ID", "Product ID", "Date", "Money Earned");

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] transaction = lines[i].Split('|');
                    if (transaction.Length >= 4)
                    {
                        transactionTable.AddRow(
                            transaction[0].Trim(),
                            transaction[1].Trim(),
                            transaction[2].Trim(),
                            transaction[3].Trim()
                            );
                    }
                }
                Console.WriteLine(transactionTable);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Press ENTER to continue");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
            }
        }

        public void viewProductRequest()
        {
            Console.Clear();
            string viewRequest = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers\{userID}\requestProduct.txt";
            if (File.Exists(viewRequest))
            {
                string[] lines = File.ReadAllLines(viewRequest);
                if (lines.Length == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No products requested.");
                    Console.ResetColor();
                }
                var requestTable = new Table("Name", "Contact Number", "Product Name", "Quantity", "Date of Order", "Date of Pick-up");

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] request = lines[i].Split('|');
                    if (request.Length >= 6)
                    {
                        requestTable.AddRow(
                            request[0].Trim(),
                            request[1].Trim(),
                            request[2].Trim(),
                            request[3].Trim(),
                            request[4].Trim(),
                            request[5].Trim()
                            );
                    }
                }
                Console.WriteLine(requestTable);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Press ENTER to continue");
                Console.ResetColor();
                Console.ReadKey();
                Console.Clear();
            }
        }

        public void viewProductRating()
        {
            Console.Clear();
            string viewFeedback = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers\{userID}\costumerFeedback.txt";
            using (StreamReader reader = File.OpenText(viewFeedback))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press ENTER to continue");
            Console.ResetColor();
            Console.ReadKey();
            Console.Clear();
        }

        public override void Register()
        {
            while (repeat)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Welcome! Please register an account.");
                    Console.ResetColor();
                    Console.Write("Enter user ID: ");
                    userID = int.Parse(Console.ReadLine());

                    Console.Write("Enter password: ");
                    password = Console.ReadLine();

                    Console.Write("Confirm Passowrd: ");
                    string confirmPass = Console.ReadLine();

                    if (confirmPass != password)
                    {
                        Console.WriteLine("Does not match");
                        break;
                    }
                    else
                    {
                        password = confirmPass;
                    }

                    //base folder for farmers
                    string baseFolder = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers";
                    Directory.CreateDirectory(baseFolder);

                    //specific folders for each farmers
                    string newFolder = Path.Combine(baseFolder, $"{userID}");
                    if (Directory.Exists(newFolder))
                    {
                        Console.WriteLine("File not created.");
                        continue;
                    }

                    Directory.CreateDirectory(newFolder);//creates a new folder

                    //this creates a txt file for farmers information
                    string fileFarmers_Information = Path.Combine(newFolder, $"{userID}.txt");
                    string productFile = Path.Combine(newFolder, $"{userID}_Product.txt");
                    string transactionHistoryFile = Path.Combine(newFolder, $"transactionHistory.txt");
                    string costumerFeedbackFile = Path.Combine(newFolder, $"costumerFeedback.txt");
                    string requestProduct = Path.Combine(newFolder, $"requestProduct.txt");

                    if (File.Exists(fileFarmers_Information))//Checks the same user ID
                    {
                        Console.WriteLine("The user ID has already been taken. Please create enter new ID");
                    }
                    else
                    {
                        Console.Write("Enter Name: ");
                        name = Console.ReadLine();
                        Console.Write("Contact Information: ");
                        contactInfo = Console.ReadLine();
                        Console.Write("Enter address(Baranggay, City): ");
                        address = Console.ReadLine();
                        Console.Write("Store name: ");
                        string storeName = Console.ReadLine();

                        repeat = false;
                        using (StreamWriter writer = File.AppendText(fileFarmers_Information))
                        {
                            writer.WriteLine("Farmer's Information");
                            writer.WriteLine($"Name: {name}");
                            writer.WriteLine($"User ID: {userID}");
                            writer.WriteLine($"Contact Information: {contactInfo}");
                            writer.WriteLine($"Address: {address}");
                            writer.WriteLine($"Store Name: {storeName}");
                            writer.WriteLine($"Password: {password}");

                        }
                        using (StreamWriter productWriter = File.AppendText(productFile))
                        {
                            productWriter.WriteLine("Product | Quantity(in Kilograms) | Price(per Kilograms) | Category | Product ID | Day Harvested | Expiry Date");
                        }
                        using (StreamWriter historyTransaction = File.AppendText(transactionHistoryFile))
                        {
                            historyTransaction.WriteLine("Transaction ID | Product ID | Date | Money Earned");
                        }
                        using (StreamWriter feedback = File.AppendText(costumerFeedbackFile))
                        {
                            feedback.WriteLine("Customer Feedback: ");
                            feedback.WriteLine();
                        }
                        using (StreamWriter request = File.AppendText(requestProduct))
                        {
                            request.WriteLine("Name | Contact Number | Product Name | Quantity (in Kg) | Date of Order | Date of Pick-up");
                        }
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input.");
                }
            }
        }

        public override void UpdateInfo()
        {
            repeat = true;

            while (repeat)
            {
                try
                {
                    Console.Clear();
                    Console.Write("Enter your User ID to update information: ");
                    int id = int.Parse(Console.ReadLine());

                    string folderExisting = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers";
                    string folderPathOld = Path.Combine(folderExisting, $"{id}");
                    string fileFarmers_Information_Old = Path.Combine(folderPathOld, $"{id}.txt");
                    string fileFarmers_Product_Old = Path.Combine(folderPathOld, $"{id}_Product.txt");

                    if (!File.Exists(fileFarmers_Information_Old))
                    {
                        Console.WriteLine("File cannot be found!");
                        return;
                    }

                    string[] changeInfo = File.ReadAllLines(fileFarmers_Information_Old);
                    var consumerTable = new Table("Option", "Action");
                    consumerTable.AddRow("0", "Close")
                                 .AddRow("1", "User Id")
                                 .AddRow("2", "Change Name")
                                 .AddRow("3", "Change Contact Information")
                                 .AddRow("4", "Change Password");
                    Console.WriteLine(consumerTable.ToString());
                    Console.Write("Choice: ");
                    int subchoice = int.Parse(Console.ReadLine());
                    Console.WriteLine();

                    switch (subchoice)
                    {
                        case 1:
                            Console.Write("Enter new User ID: ");
                            int newID = int.Parse(Console.ReadLine());

                            //changes the folder name and the txt name
                            string folderPathNew = Path.Combine(folderExisting, $"{newID}");
                            string fileFarmers_Information_New = Path.Combine(folderPathNew, $"{newID}.txt");
                            string fileFarmers_Products_New = Path.Combine(folderPathNew, $"{newID}_Product.txt");

                            if (Directory.Exists(folderPathNew))
                            {
                                Console.WriteLine("That User ID already exists. Choose a different one.");
                                break;
                            }

                            //Update ID text inside file
                            changeInfo[3] = $"User ID: {newID}";
                            File.WriteAllLines(fileFarmers_Information_Old, changeInfo);

                            //Rename the folder
                            Directory.Move(folderPathOld, folderPathNew);

                            //Rename the file inside the new folder
                            string oldFileMoved = Path.Combine(folderPathNew, $"{id}.txt");
                            string oldFileProductMoved = Path.Combine(folderPathNew, $"{id}_Product.txt");

                            File.Move(oldFileMoved, fileFarmers_Information_New);
                            File.Move(oldFileProductMoved, fileFarmers_Products_New);
                            Console.WriteLine($"User ID updated successfully!");
                            repeat = false;
                            break;

                        case 2:
                            Console.Write("Enter new Name: ");
                            name = Console.ReadLine();
                            changeInfo[1] = $"Name: {name}";
                            File.WriteAllLines(fileFarmers_Information_Old, changeInfo);
                            Console.WriteLine("Name updated successfully!");
                            repeat = false;
                            break;

                        case 3:
                            Console.Write("Enter new Contact Information: ");
                            contactInfo = Console.ReadLine();
                            changeInfo[2] = $"Contact Information: {contactInfo}";
                            File.WriteAllLines(fileFarmers_Information_Old, changeInfo);
                            Console.WriteLine("Contact Information updated successfully!");
                            repeat = false;
                            break;

                        case 4:
                            string basePath = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers\{id}";
                            string filePath = Path.Combine(basePath, $"{id}.txt");
                            string newPassword = "";
                            string currentPass = "";
                            while (repeat)
                            {
                                Console.Write("Enter current password: ");
                                string enterPass = Console.ReadLine();


                                if (File.Exists(filePath))
                                {
                                    string[] line = File.ReadAllLines(filePath);
                                    foreach (string found in line)
                                    {
                                        if (found.StartsWith("Password: "))
                                        {
                                            currentPass = found.Replace("Password: ", "").Trim();
                                        }
                                    }
                                }

                                if (enterPass != currentPass)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Incorrect Password. Please enter again.");
                                    Console.ResetColor();
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Console.Write("Enter new password: ");
                                    newPassword = Console.ReadLine();

                                    string[] lines = File.ReadAllLines(filePath);

                                    for (int i = 1; i < lines.Length; i++)
                                    {
                                        if (lines[i].Contains($"Password: {currentPass}"))
                                        {
                                            lines[i] = $"Password: {newPassword}";
                                            repeat = false;
                                            break;
                                        }
                                    }
                                    File.WriteAllLines(filePath, lines);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Password successfully changed.");
                                    Console.ResetColor();
                                    repeat = false;

                                }

                            }
                            break;

                        case 0:
                            repeat = false;
                            Console.Clear();
                            break;

                        default:
                            Console.WriteLine("Invalid choice.");
                            break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input. Please enter valid data.");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"File error: {ex.Message}");
                }
            }
        }
    }

    public class Consumer : User
    {
        bool repeat = true;


        public void ViewStores()
        {
            string directoryFarmers = @"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers";

            if (!Directory.Exists(directoryFarmers))
            {
                Console.WriteLine("No farmer directory found.");
                return;
            }

            var stores = new List<(string StoreName, string Owner, string Contact, string Location, string Category, string FolderPath)>();

            foreach (string folder in Directory.GetDirectories(directoryFarmers))
            {
                string fileName = Path.GetFileName(folder);
                string infoFile = Path.Combine(folder, $"{fileName}.txt");

                if (!File.Exists(infoFile))
                    continue;

                string[] lines = File.ReadAllLines(infoFile);
                string name = "", contactInfo = "", location = "", storeName = "";

                foreach (string line in lines)
                {
                    if (line.StartsWith("Name: "))
                        name = line.Replace("Name: ", "").Trim();
                    else if (line.StartsWith("Contact Information: "))
                        contactInfo = line.Replace("Contact Information:", "").Trim();
                    else if (line.StartsWith("Address: "))
                        location = line.Replace("Address: ", "").Trim();
                    else if (line.StartsWith("Store Name: "))
                        storeName = line.Replace("Store Name: ", "").Trim();
                }

                // Collect product categories
                string productFile = Path.Combine(folder, $"{fileName}_Product.txt");
                List<string> categories = new List<string>();

                if (File.Exists(productFile))
                {
                    string[] productLines = File.ReadAllLines(productFile);

                    for (int i = 0; i < productLines.Length; i++)
                    {
                        string[] parts = productLines[i].Split('|');
                        if (parts.Length >= 4)
                        {
                            string category = parts[3].Trim();

                            // Skip the header or any line that literally contains "Category"
                            if (category.Equals("Category", StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (!categories.Contains(category))
                            {
                                categories.Add(category);
                            }
                        }
                    }
                }

                string categoryDisplay = categories.Count > 0 ? string.Join(", ", categories) : "No products listed yet.";

                stores.Add((storeName, name, contactInfo, location, categoryDisplay, folder));
            }

            if (stores.Count == 0)
            {
                Console.WriteLine("No stores available.");
                return;
            }

            // Sort by store name alphabetically
            stores = stores.OrderBy(s => s.StoreName).ToList();

            Console.Clear();
            Console.WriteLine("========== AVAILABLE STORES ==========\n");

            var storeTable = new Table("Store #", "Store Name", "Owner", "Contact Info", "Location", "Categories");

            for (int i = 0; i < stores.Count; i++)
            {
                var s = stores[i];
                storeTable.AddRow(i + 1, s.StoreName, s.Owner, s.Contact, s.Location, s.Category);
            }

            Console.WriteLine(storeTable);
            Console.WriteLine();

            // Selection logic
            while (true)
            {
                Console.Write("\n0. Back\nSelect a store number to view products (1 - " + stores.Count + "): ");
                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input. Try again.");
                    continue;
                }

                if (choice == 0)
                {
                    Console.Clear();
                    break;
                }

                if (choice >= 1 && choice <= stores.Count)
                {
                    var selectedStore = stores[choice - 1];
                    string folder = selectedStore.FolderPath;
                    string fileName = Path.GetFileName(folder);
                    string productFile = Path.Combine(folder, $"{fileName}_Product.txt");
                    string requestProduct = Path.Combine(folder, "requestProduct.txt");

                    Console.WriteLine($"\nProducts available in {selectedStore.StoreName}:");
                    Console.WriteLine("----------------------------------");

                    if (File.Exists(productFile))
                    {
                        string[] products = File.ReadAllLines(productFile);

                        if (products.Length == 0)
                        {
                            Console.WriteLine("No products found.");
                            continue;
                        }

                        var productTable = new Table("Product Name", "Quantity(in Kg)", "Price(per Kg)", "Category", "ID", "Harvest Date", "Expiry Date");

                        for (int i = 1; i < products.Length; i++) // start from 1 to skip the header line
                        {
                            string[] parts = products[i].Split('|');
                            if (parts.Length >= 7)
                            {
                                productTable.AddRow(
                                    parts[0].Trim(),
                                    parts[1].Trim(),
                                    parts[2].Trim(),
                                    parts[3].Trim(),
                                    parts[4].Trim(),
                                    parts[5].Trim(),
                                    parts[6].Trim()
                                );
                            }
                        }

                        Console.WriteLine(productTable);
                        Console.WriteLine();

                        Console.Write("0.Back\nDo you want to request/reserve a product [y/n]? ");
                        char request = char.ToLower(Console.ReadKey().KeyChar);
                        Console.WriteLine();

                        if (request == 'y')
                        {
                            requestProducts(requestProduct);
                        }
                        else if (request == '0' || request == 'n')
                        {
                            Console.Clear();
                            Console.WriteLine(storeTable);
                            continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine("No products found for this store.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid store number.");
                }
            }
        }

        public void requestProducts(string requestProduct)
        {
            string date = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
            Console.Write("Enter name: ");
            string name = Console.ReadLine();

            Console.Write("Enter contact number: ");
            string contactNumber = Console.ReadLine();

            Console.Write("Enter product: ");
            string productName = Console.ReadLine();

            Console.Write("Enter quantity(in kilograms): ");
            double quantity = double.Parse(Console.ReadLine());

            Console.WriteLine("Date of pickup(mm/dd/2025)");
            Console.Write("Enter month: ");
            int month = int.Parse(Console.ReadLine());

            Console.Write("Enter day: ");
            int day = int.Parse(Console.ReadLine());


            Console.WriteLine("Request has been sent. Thank you!");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("!!!Reminder: Products that are not claimed after 24 hours of pick up date will be cancelled!!!");
            Console.ResetColor();

            using (StreamWriter writer = File.AppendText(requestProduct))
            {
                writer.WriteLine($"{name} | {contactNumber} | {productName} | {quantity} | {date} | {month}/{day}/2025");
            }

        }

        public void rateProduct()
        {
            Console.Write("Enter the Product ID you want to rate: ");
            string inputID = Console.ReadLine();

            string farmersDirectory = @"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers";
            bool found = false;
            string productName = "";
            string amountBought = "";
            string farmerFolder = "";
            string ownerName = "";
            string storeName = "";

            foreach (string folder in Directory.GetDirectories(farmersDirectory))//it is going to read every farmers registered in the farmer's file
            {
                string fileName = Path.GetFileName(folder);
                string productFile = Path.Combine(folder, $"{fileName}_Product.txt");
                string transactionFile = Path.Combine(folder, "transactionHistory.txt");
                string FarmerInfo = Path.Combine(folder, $"{fileName}.txt");

                if (File.Exists(productFile))
                {
                    string[] productLines = File.ReadAllLines(productFile);
                    for (int i = 1; i < productLines.Length; i++)
                    {
                        if (productLines[i].Contains(inputID))
                        {
                            string[] parts = productLines[i].Split('|');
                            if (parts.Length >= 2)
                            {
                                productName = parts[0].Trim();
                                farmerFolder = folder;
                                found = true;
                                break;
                            }
                        }
                    }
                }

                if (File.Exists(FarmerInfo))
                {
                    string[] info = File.ReadAllLines(FarmerInfo);
                    foreach (string line in info)
                    {
                        if (line.StartsWith("Name: "))
                        {
                            ownerName = line.Replace("Name: ", "").Trim();
                        }
                        else if (line.StartsWith("Store Name: "))
                        {
                            storeName = line.Replace("Store Name: ", "").Trim();
                        }
                    }
                }

            }

            if (!found)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Product not found in any farmer's list.");
                Console.ResetColor();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nProduct Found!");
            Console.ResetColor();
            Console.WriteLine($"Product ID: {inputID}");
            Console.WriteLine($"Product Name: {productName}");
            Console.WriteLine($"Farmer's Name: {ownerName}");
            Console.WriteLine($"Store Name: {storeName}");

            int rating;
            do
            {
                Console.Write("Rate the product (1–5): ");
                rating = int.Parse(Console.ReadLine());
                if (rating < 1 || rating > 5)
                {
                    Console.WriteLine("Please enter a valid rating between 1 and 5.");
                }
            } while (rating < 1 || rating > 5);

            Console.Write("Enter your feedback: ");
            string comment = Console.ReadLine();

            string consumerInfoFile = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Consumers\{userID}\{userID}.txt";
            string consumerName = "Unknown Consumer";
            if (File.Exists(consumerInfoFile))
            {
                foreach (string line in File.ReadAllLines(consumerInfoFile))
                {
                    if (line.StartsWith("Name: "))
                    {
                        consumerName = line.Replace("Name: ", "").Trim();
                        break;
                    }
                }
            }

            string feedbackFile = Path.Combine(farmerFolder, "costumerFeedback.txt");
            using (StreamWriter writer = File.AppendText(feedbackFile))
            {
                writer.WriteLine($"Product ID: {inputID}");
                writer.WriteLine($"Costumer Name: {consumerName}");
                writer.WriteLine($"Product Name: {productName}");
                writer.WriteLine($"Feedback: {comment}");
                writer.WriteLine();
            }
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Thank you for your feedback! It has been submitted successfully.");
            Console.ResetColor();
        }

        public void getReceipt()
        {
            Console.Write("Enter Transaction ID: ");
            string inputID = Console.ReadLine();

            string farmersDirectory = @"C:\Users\Christian Reyniel\Documents\notepadfile\Farmers";
            string consumerTransactionFile = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Consumers\{userID}\transactionHistory.txt";

            bool found = false;
            string productID = "";
            string productName = "";
            string date = "";
            string moneySpent = "";

            foreach (string folder in Directory.GetDirectories(farmersDirectory))
            {
                string fileName = Path.GetFileName(folder);
                string transactionFile = Path.Combine(folder, "transactionHistory.txt");
                string productFile = Path.Combine(folder, $"{fileName}_Product.txt");

                if (!File.Exists(transactionFile)) continue;

                string[] transactionLines = File.ReadAllLines(transactionFile);
                for (int i = 1; i < transactionLines.Length; i++)
                {
                    string[] parts = transactionLines[i].Split('|');
                    if (parts.Length >= 4 && parts[0].Trim() == inputID)
                    {
                        productID = parts[1].Trim();
                        date = parts[2].Trim();
                        moneySpent = parts[3].Trim();
                        found = true;

                        if (File.Exists(productFile))
                        {
                            string[] productLines = File.ReadAllLines(productFile);
                            foreach (string pLine in productLines)
                            {
                                if (pLine.Contains(productID))
                                {
                                    string[] pParts = pLine.Split('|');
                                    if (pParts.Length >= 2)
                                        productName = pParts[0].Trim();
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }

                if (found) break;
            }

            if (!found)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Transaction not found in any farmer's records.");
                Console.ResetColor();
                return;
            }

            using (StreamWriter writer = File.AppendText(consumerTransactionFile))
            {
                writer.WriteLine($"{inputID} | {productID} | {productName} | {date} | {moneySpent}");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nReceipt added successfully!");
            Console.ResetColor();
            Console.WriteLine($"Transaction ID: {inputID}");
            Console.WriteLine($"Product ID: {productID}");
            Console.WriteLine($"Product Name: {productName}");
            Console.WriteLine($"Date: {date}");
            Console.WriteLine($"Money Spent: {moneySpent}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press ENTER to continue");
            Console.ResetColor();
            Console.ReadKey();
            Console.Clear();

        }

        public void viewTransactionHistory()
        {
            Console.Clear();
            string consumerTransactionFile = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Consumers\{userID}\transactionHistory.txt";

            string[] history = File.ReadAllLines(consumerTransactionFile);
            var receiptTable = new Table("Transaction ID", "Product ID", "Product", "Date", "Money Paid");

            if (history.Length == 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No transaction history.");
                Console.ResetColor();
            }

            for (int i = 1; i < history.Length; i++)
            {
                string[] receiptInfo = history[i].Split('|');
                if (receiptInfo.Length >= 4)
                {
                    receiptTable.AddRow(
                        receiptInfo[0],
                        receiptInfo[1],
                        receiptInfo[2],
                        receiptInfo[3],
                        receiptInfo[4]
                        );
                }
            }
            Console.WriteLine(receiptTable);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press ENTER to continue");
            Console.ResetColor();
            Console.ReadKey();
            Console.Clear();

        }
        public override void Register()
        {
            while (repeat)
            {
                try
                {
                    Console.WriteLine("Welcome! Please register an account.");
                    Console.Write("Enter user ID: ");
                    userID = int.Parse(Console.ReadLine());

                    //base folder for consumers
                    string baseFolder = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Consumers";
                    Directory.CreateDirectory(baseFolder);

                    //specific folders for each consumers
                    string newFolder = Path.Combine(baseFolder, $"{userID}");
                    if (Directory.Exists(newFolder))
                    {
                        Console.WriteLine("User ID is taken.");
                        continue;
                    }

                    //this creates a txt file for consumers information
                    string fileConsumers_Information = Path.Combine(newFolder, $"{userID}.txt");
                    string transactionHistoryFile = Path.Combine(newFolder, $"transactionHistory.txt");

                    if (File.Exists(fileConsumers_Information))//Checks the same user ID
                    {
                        Console.WriteLine("The user ID has already been taken. Please create enter new ID");
                    }
                    else
                    {
                        Directory.CreateDirectory(newFolder);//creates a new folder
                        Console.Write("Enter Name: ");
                        name = Console.ReadLine();

                        Console.Write("Enter password: ");
                        password = Console.ReadLine();

                        Console.Write("Confirm Passowrd: ");
                        string confirmPass = Console.ReadLine();

                        if (confirmPass != password)
                        {
                            Console.WriteLine("Does not match");
                        }
                        else
                        {
                            password = confirmPass;
                        }
                        Console.Write("Contact Information: ");
                        contactInfo = Console.ReadLine();
                        Console.Write("Enter address(Baranggay, City): ");
                        address = Console.ReadLine();

                        repeat = false;
                        using (StreamWriter writer = File.AppendText(fileConsumers_Information))
                        {
                            writer.WriteLine("Consumer's Information");
                            writer.WriteLine($"Name: {name}");
                            writer.WriteLine($"Contact Information: {contactInfo}");
                            writer.WriteLine($"User ID: {userID}");
                            writer.WriteLine($"Address: {address}");
                            writer.WriteLine($"Password: {password}");

                        }
                        using (StreamWriter historyTransaction = File.AppendText(transactionHistoryFile))
                        {
                            historyTransaction.WriteLine("Transaction ID | Product ID | Product | Date | Money Paid");
                        }
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input.");
                }
            }
        }
        public override void UpdateInfo()
        {
            repeat = true;

            while (repeat)
            {
                try
                {
                    Console.Write("Enter your user ID to update information: ");
                    int id = int.Parse(Console.ReadLine());

                    string folderExisting = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Consumers";
                    string folderPathOld = Path.Combine(folderExisting, $"{id}");
                    string fileConsumers_Information_Old = Path.Combine(folderPathOld, $"{id}.txt");
                    string fileConsumers_Product_Old = Path.Combine(folderPathOld, $"{id}_Product.txt");

                    if (!File.Exists(fileConsumers_Information_Old))
                    {
                        Console.WriteLine("File cannot be found!");
                        return;
                    }

                    string[] changeInfo = File.ReadAllLines(fileConsumers_Information_Old);

                    var consumerTable = new Table("Option", "Action");
                    consumerTable.AddRow("0", "Close")
                                 .AddRow("1", "User Id")
                                 .AddRow("2", "Change Name")
                                 .AddRow("3", "Change Contact Information")
                                 .AddRow("4", "Change Password");
                    Console.WriteLine(consumerTable.ToString());
                    Console.Write("Choice: ");
                    int subchoice = int.Parse(Console.ReadLine());
                    Console.WriteLine();

                    switch (subchoice)
                    {
                        case 1:
                            Console.Write("Enter new User ID: ");
                            int newID = int.Parse(Console.ReadLine());

                            //changes the folder name and the txt name
                            string folderPathNew = Path.Combine(folderExisting, $"{newID}");
                            string fileFarmers_Information_New = Path.Combine(folderPathNew, $"{newID}.txt");
                            string fileFarmers_Products_New = Path.Combine(folderPathNew, $"{newID}_Product.txt");

                            if (Directory.Exists(folderPathNew))
                            {
                                Console.WriteLine("That User ID already exists. Choose a different one.");
                                break;
                            }

                            //Update ID text inside file
                            changeInfo[3] = $"User ID: {newID}";
                            File.WriteAllLines(fileConsumers_Information_Old, changeInfo);

                            //Rename the folder
                            Directory.Move(folderPathOld, folderPathNew);

                            //Rename the file inside the new folder
                            string oldFileMoved = Path.Combine(folderPathNew, $"{id}.txt");
                            string oldFileProductMoved = Path.Combine(folderPathNew, $"{id}_Product.txt");

                            File.Move(oldFileMoved, fileFarmers_Information_New);
                            File.Move(oldFileProductMoved, fileFarmers_Products_New);
                            Console.WriteLine($"User ID updated successfully!");
                            repeat = false;
                            break;

                        case 2:
                            Console.Write("Enter new Name: ");
                            name = Console.ReadLine();
                            changeInfo[1] = $"Name: {name}";
                            File.WriteAllLines(fileConsumers_Information_Old, changeInfo);
                            Console.WriteLine("Name updated successfully!");
                            repeat = false;
                            break;

                        case 3:
                            Console.Write("Enter new Contact Information: ");
                            contactInfo = Console.ReadLine();
                            changeInfo[2] = $"Contact Information: {contactInfo}";
                            File.WriteAllLines(fileConsumers_Information_Old, changeInfo);
                            Console.WriteLine("Contact Information updated successfully!");
                            repeat = false;
                            break;

                        case 4:
                            string basePath = $@"C:\Users\Christian Reyniel\Documents\notepadfile\Consumers\{id}";
                            string filePath = Path.Combine(basePath, $"{id}.txt");
                            string newPassword = "";
                            string currentPass = "";
                            while (repeat)
                            {
                                Console.Write("Enter current password: ");
                                string enterPass = Console.ReadLine();


                                if (File.Exists(filePath))
                                {
                                    string[] line = File.ReadAllLines(filePath);
                                    foreach (string found in line)
                                    {
                                        if (found.StartsWith("Password: "))
                                        {
                                            currentPass = found.Replace("Password: ", "").Trim();
                                        }
                                    }
                                }

                                if (enterPass != currentPass)
                                {
                                    Console.WriteLine("Incorrect Password. Please enter again.");
                                    Console.WriteLine();
                                }
                                else
                                {
                                    Console.Write("Enter new password: ");
                                    newPassword = Console.ReadLine();

                                    string[] lines = File.ReadAllLines(filePath);

                                    for (int i = 1; i < lines.Length; i++)
                                    {
                                        if (lines[i].Contains($"Password: {currentPass}"))
                                        {
                                            lines[i] = $"Password: {newPassword}";
                                            repeat = false;
                                            break;
                                        }
                                    }
                                    File.WriteAllLines(filePath, lines);
                                    Console.WriteLine("Password successfully changed.");
                                    repeat = false;

                                }

                            }
                            break;

                        case 0:
                            repeat = false;
                            Console.Clear();
                            break;


                        default:
                            Console.WriteLine("Invalid choice.");
                            break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid input. Please enter valid data.");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"File error: {ex.Message}");
                }
            }
        }
    }



    internal class Program
    {
        static void Main(string[] args)
        {
            bool repeat = true;
            try
            {
                Console.WriteLine("Welcome to the Local Farmers’ Product Management System!");
                Console.WriteLine("Supporting local farmers, one harvest at a time.\n");

                var mainTable = new Table("Option", "Description");
                mainTable.AddRow("1", "Login");
                mainTable.AddRow("2", "Register");
                Console.WriteLine(mainTable.ToString());

            Start:
                Console.Write("Enter your choice: ");
                string choiceInput = Console.ReadLine();
                int choice;

                if (!int.TryParse(choiceInput, out choice))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Please enter a NUMBER.\n");
                    Console.ResetColor();
                    goto Start;
                }

                Console.WriteLine();

                if (choice == 1)
                {
                choice:
                    Console.Write("Are you a Farmer or Consumer? ");
                    string role = Console.ReadLine().ToLower();
                    if (role != "farmer" && role != "consumer")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid Input!");
                        Console.ResetColor();
                        goto choice;
                    }
                userID:
                    Console.Write("Enter User ID: ");
                    string enteredID = Console.ReadLine();

                    string basePath = $@"C:\Users\Christian Reyniel\Documents\notepadfile\{(role == "farmer" ? "Farmers" : "Consumers")}\{enteredID}";
                    string filePath = Path.Combine(basePath, $"{enteredID}.txt");

                    if (!File.Exists(filePath))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid User ID");
                        Console.ResetColor();
                        goto userID;
                    }
                    while (repeat)
                    {
                        Console.Write("Enter password: ");
                        string password = Console.ReadLine();

                        string foundpassword = "";

                        if (File.Exists(filePath))
                        {
                            string[] findPassword = File.ReadAllLines(filePath);
                            foreach (string found in findPassword)
                            {
                                if (found.StartsWith("Password: "))
                                {
                                    foundpassword = found.Replace("Password: ", "").Trim();
                                    break;
                                }
                            }
                        }

                        if (password != foundpassword)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Incorrect Password\n");
                            Console.ResetColor();
                        }
                        else
                        {
                            string name = "";

                            if (File.Exists(filePath))
                            {
                                string[] readLine = File.ReadAllLines(filePath);
                                foreach (string line in readLine)
                                {
                                    if (line.StartsWith("Name: "))
                                    {
                                        name = line.Replace("Name: ", "").Trim();
                                    }
                                }
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"\nLogin successful!");
                                Console.ResetColor();
                                Console.WriteLine($"Welcome, {name}.\n");

                                if (role == "farmer")
                                {
                                    Farmer farmer = new Farmer();
                                    farmer.userID = int.Parse(enteredID);

                                    while (repeat)
                                    {
                                        Console.WriteLine("\nFarmer Dashboard:");
                                        var farmerTable = new Table("Option", "Action");
                                        farmerTable.AddRow("1", "Update Information")
                                                   .AddRow("2", "Add Product")
                                                   .AddRow("3", "Deduct/Remove Product")
                                                   .AddRow("4", "View Product")
                                                   .AddRow("5", "View Transaction History")
                                                   .AddRow("6", "View Requests")
                                                   .AddRow("7", "View Feedback")
                                                   .AddRow("0", "Exit");

                                        Console.WriteLine(farmerTable.ToString());
                                        Console.Write("Choice: ");

                                        string subInput = Console.ReadLine();
                                        int subChoice;

                                    
                                        if (!int.TryParse(subInput, out subChoice))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Invalid input. Please enter a NUMBER.\n");
                                            Console.ResetColor();
                                            continue;
                                        }

                                        Console.Clear();

                                        switch (subChoice)
                                        {
                                            case 1: farmer.UpdateInfo(); break;
                                            case 2: farmer.AddProduct(); break;
                                            case 3: farmer.removeProduct(); break;
                                            case 4: farmer.DisplayProduct(); break;
                                            case 5: farmer.viewTransactionHistory(); break;
                                            case 6: farmer.viewProductRequest(); break;
                                            case 7: farmer.viewProductRating(); break;
                                            case 0:
                                                Console.WriteLine("Thank you for using this app!");
                                                repeat = false;
                                                break;
                                        }
                                    }
                                }
                                else if (role == "consumer")
                                {
                                    Consumer consumer = new Consumer();
                                    consumer.userID = int.Parse(enteredID);

                                    while (repeat)
                                    {
                                        Console.WriteLine("\nConsumer Dashboard:");
                                        var consumerTable = new Table("Option", "Action");
                                        consumerTable.AddRow("1", "View Stores")
                                                     .AddRow("2", "Rate Products")
                                                     .AddRow("3", "Get Receipt")
                                                     .AddRow("4", "View Receipt")
                                                     .AddRow("5", "Update Information")
                                                     .AddRow("0", "Exit");

                                        Console.WriteLine(consumerTable.ToString());
                                        Console.Write("Choice: ");

                                        string subInput = Console.ReadLine();
                                        int subChoice;

                                        if (!int.TryParse(subInput, out subChoice))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Invalid input. Please enter a NUMBER.\n");
                                            Console.ResetColor();
                                            continue;
                                        }

                                        Console.Clear();

                                        switch (subChoice)
                                        {
                                            case 1: consumer.ViewStores(); break;
                                            case 2: consumer.rateProduct(); break;
                                            case 3: consumer.getReceipt(); break;
                                            case 4: consumer.viewTransactionHistory(); break;
                                            case 5: consumer.UpdateInfo(); break;
                                            case 0:
                                                Console.WriteLine("Thank you for using this app!");
                                                repeat = false;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (choice == 2)
                {
                choice:
                    Console.Write("Are you a Farmer or Consumer? ");
                    string user = Console.ReadLine().ToLower();
                    if (user == "farmer")
                    {
                        Console.Clear();
                        User farmer = new Farmer();
                        farmer.Register();
                    }
                    else if (user == "consumer")
                    {
                        Console.Clear();
                        User consumer = new Consumer();
                        consumer.Register();

                    }
                    else if (user != "farmer" && user != "consumer")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid Input!");
                        Console.ResetColor();
                        goto choice;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid Choice");
                    Console.ResetColor();
                    goto Start;
                }
            }
            catch (FormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input.");
                Console.ResetColor();
                repeat = true;
            }
        }
    }

}





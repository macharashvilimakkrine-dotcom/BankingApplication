using System;
using System.Collections.Generic;
using System.IO; // ფაილებთან სამუშაოდ
using System.Linq;

namespace BankingApplication
{
    // ახალი მოთხოვნების შესაბამისი მოდელი
    public class Account
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CVC { get; set; }
        public string PinCode { get; set; }
        public double Balance { get; set; }
        public bool IsBlocked { get; set; }
        public List<string> TransactionHistory { get; set; } // ოპერაციების ისტორია

        public Account(string firstName, string lastName, string cardNumber, string expirationDate, string cvc, string pinCode, double balance)
        {
            FirstName = firstName;
            LastName = lastName;
            CardNumber = cardNumber;
            ExpirationDate = expirationDate;
            CVC = cvc;
            PinCode = pinCode;
            Balance = balance;
            IsBlocked = false;
            TransactionHistory = new List<string>();
        }

        public string FullName => $"{FirstName} {LastName}";
    }

    class Program
    {
        static List<Account> bankAccounts = new List<Account>();
        static Account currentAccount = null;
        
        static string filePath = "accounts_data.txt"; // მონაცემების ფაილი
        static string logFilePath = "logs.txt";        // ლოგების ფაილი

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // ქართული შრიფტის მხარდაჭერა
            
            // 1. მონაცემების ჩატვირთვა ფაილიდან
            LoadDataFromFile();

            LogAction("სისტემა ამოქმედდა.");

            // უსასრულო ციკლი, რათა ყოველი სესიის მერე სისტემა დაბრუნდეს საწყის პოზიციაზე
            while (true)
            {
                currentAccount = null;
                Console.Clear();
                Console.WriteLine("=== კეთილი იყოს თქვენი მობრძანება Commschool ბანკში ===");

                // 2. ავტორიზაცია
                if (LoginProcess())
                {
                    // 3. მენიუ
                    ShowAtmMenu();
                }

                Console.WriteLine("\nსესია დასრულდა. საწყის პოზიციაზე დასაბრუნებლად დააჭირეთ ნებისმიერ ღილაკს...");
                Console.ReadKey();
            }
        }

        // ფუნქცია, რომელიც წერს ლოგებს ფაილში
        static void LogAction(string message)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch { }
        }

        // ფაილის შენახვის ფუნქცია ყოველი ბალანსის ან პინის ცვლილებისას
        static void SaveDataToFile()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (var acc in bankAccounts)
                {
                    // თითოეულ ხაზზე ვინახავთ ყველა ველს მძიმეებით გამოყოფილს
                    string line = $"{acc.FirstName},{acc.LastName},{acc.CardNumber},{acc.ExpirationDate},{acc.CVC},{acc.PinCode},{acc.Balance},{acc.IsBlocked}";
                    lines.Add(line);
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                LogAction($"ფაილში შენახვის შეცდომა: {ex.Message}");
            }
        }

        static void LoadDataFromFile()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    bankAccounts.Clear();
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length >= 7)
                        {
                            string firstName = parts[0];
                            string lastName = parts[1];
                            string cardNumber = parts[2];
                            string expirationDate = parts[3];
                            string cvc = parts[4];
                            string pinCode = parts[5];
                            double balance = double.Parse(parts[6]);

                            var acc = new Account(firstName, lastName, cardNumber, expirationDate, cvc, pinCode, balance);
                            if (parts.Length == 8)
                            {
                                acc.IsBlocked = bool.Parse(parts[7]);
                            }
                            bankAccounts.Add(acc);
                        }
                    }
                }
                else
                {
                    // თუ ფაილი არ არსებობს, ვქმნით მას ზუსტად შენი ახალი მონაცემებით!
                    string defaultLine = "Makrine,Macharashvili,1234-5678-9012-3456,12/25,123,1234,1500.50,False";
                    File.WriteAllText(filePath, defaultLine + Environment.NewLine);
                    
                    var defaultAcc = new Account("Makrine", "Macharashvili", "1234-5678-9012-3456", "12/25", "123", "1234", 1500.50);
                    bankAccounts.Add(defaultAcc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ფაილის წაკითხვისას დაფიქსირდა შეცდომა: {ex.Message}");
            }
        }

        static bool LoginProcess()
        {
            int pinAttempts = 0;
            Console.Write("\nგთხოვთ, შეიყვანოთ ბარათის ნომერი (მაგ: 1234-5678-9012-3456): ");
            string enteredCard = Console.ReadLine()?.Trim();

            currentAccount = bankAccounts.FirstOrDefault(a => a.CardNumber == enteredCard);

            if (currentAccount == null)
            {
                Console.WriteLine("შეცდომა: ბარათი ვერ მოიძებნა!");
                LogAction($"ავტორიზაციის მცდელობა ჩავარდა. ბარათი ვერ მოიძებნა: {enteredCard}");
                return false;
            }

            if (currentAccount.IsBlocked)
            {
                Console.WriteLine("ეს ბარათი დაბლოკილია! მიმართეთ ბანკს.");
                LogAction($"მცდელობა დაბლოკილ ბარათზე: {enteredCard}");
                return false;
            }

            Console.Write("გთხოვთ, შეიყვანოთ ბარათის მოქმედების ვადა (MM/YY): ");
            string enteredExpiry = Console.ReadLine()?.Trim();

            if (currentAccount.ExpirationDate != enteredExpiry)
            {
                Console.WriteLine("შეცდომა: ბარათის მოქმედების ვადა არასწორია!");
                LogAction($"არასწორი ვადა ბარათისთვის: {enteredCard}");
                return false;
            }

            while (pinAttempts < 3)
            {
                Console.Write("შეიყვანეთ 4-ნიშნა PIN კოდი: ");
                string enteredPin = Console.ReadLine()?.Trim();

                if (currentAccount.PinCode == enteredPin)
                {
                    Console.Clear();
                    Console.WriteLine($"მოგესალმებით, {currentAccount.FullName}!");
                    LogAction($"მომხმარებელმა {currentAccount.FullName} გაიარა ავტორიზაცია.");
                    currentAccount.TransactionHistory.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Balance Inquiry | თანხა: 0");
                    return true;
                }
                else
                {
                    pinAttempts++;
                    Console.WriteLine($"არასწორი PIN! დარჩენილი ცდების რაოდენობა: {3 - pinAttempts}");
                    LogAction($"არასწორი PIN ბარათზე: {enteredCard}. ცდა: {pinAttempts}");
                }
            }

            currentAccount.IsBlocked = true;
            SaveDataToFile(); // ვინახავთ ბლოკირების სტატუსს
            Console.WriteLine("\nუსაფრთხოების მიზნით თქვენი ბარათი დაიბლოკა!");
            LogAction($"ბარათი {enteredCard} დაიბლოკა 3 არასწორი PIN-ის გამო.");
            return false;
        }

        static void ShowAtmMenu()
        {
            bool isRunning = true;
            while (isRunning)
            {
                Console.WriteLine("\n--- ბანკომატის მენიუ ---");
                Console.WriteLine("1. ნაშთის ნახვა");
                Console.WriteLine("2. თანხის გამოტანა ანგარიშიდან (GetAmount)");
                Console.WriteLine("3. ბოლო 5 ოპერაცია");
                Console.WriteLine("4. თანხის შეტანა ანგარიშზე (FillAmount)");
                Console.WriteLine("5. პინ კოდის შეცვლა (ChangePin)");
                Console.WriteLine("6. ვალუტის კონვერტაცია");
                Console.WriteLine("7. გასვლა");
                Console.Write("აირჩიეთ სასურველი მოქმედება (1-7): ");

                string choice = Console.ReadLine()?.Trim();

                try
                {
                    switch (choice)
                    {
                        case "1": CheckBalance(); break;
                        case "2": WithdrawCash(); break;
                        case "3": ShowLastTransactions(); break;
                        case "4": DepositCash(); break;
                        case "5": ChangePinCode(); break;
                        case "6": ConvertCurrency(); break;
                        case "7": isRunning = false; break;
                        default: Console.WriteLine("არასწორი არჩევანი! გთხოვთ აირჩიოთ 1-დან 7-მდე."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"დაფიქსირდა შეცდომა: {ex.Message}");
                    LogAction($"კრიტიკული შეცდომა მენიუში: {ex.Message}");
                }
            }
        }

        static void CheckBalance()
        {
            Console.WriteLine($"\nთქვენი მიმდინარე ბალანსია: {currentAccount.Balance} GEL");
            currentAccount.TransactionHistory.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Balance Inquiry | თანხა: 0");
            LogAction($"მომხმარებელმა {currentAccount.FullName} შეამოწმა ბალანსი.");
        }

        static void WithdrawCash()
        {
            Console.Write("\nშეიყვანეთ გამოსატანი თანხის ოდენობა: ");
            if (double.TryParse(Console.ReadLine(), out double amount))
            {
                if (amount <= 0)
                {
                    Console.WriteLine("შეცდომა: თანხა უნდა იყოს დადებითი რიცხვი!");
                }
                else if (amount > currentAccount.Balance)
                {
                    Console.WriteLine("შეცდომა: ანგარიშზე არ არის საკმარისი თანხა!");
                }
                else
                {
                    currentAccount.Balance -= amount;
                    Console.WriteLine($"ტრანზაქცია წარმატებულია! გატანილია: {amount} GEL.");
                    currentAccount.TransactionHistory.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] GetAmount | თანხა: {amount}");
                    SaveDataToFile();
                    LogAction($"მომხმარებელმა {currentAccount.FullName} გაიტანა {amount} GEL.");
                }
            }
            else
            {
                Console.WriteLine("შეცდომა: გთხოვთ შეიყვანოთ ვალიდური რიცხვი!");
            }
        }

        static void ShowLastTransactions()
        {
            Console.WriteLine("\n--- ბოლო 5 ოპერაცია ---");
            if (currentAccount.TransactionHistory.Count == 0)
            {
                Console.WriteLine("ოპერაციების ისტორია ცარიელია.");
                return;
            }

            var lastFive = currentAccount.TransactionHistory.Skip(Math.Max(0, currentAccount.TransactionHistory.Count - 5)).ToList();
            foreach (var tx in lastFive)
            {
                Console.WriteLine(tx);
            }
        }

        static void DepositCash()
        {
            Console.Write("\nშეიყვანეთ შესატანი თანხის ოდენობა: ");
            if (double.TryParse(Console.ReadLine(), out double amount))
            {
                if (amount <= 0)
                {
                    Console.WriteLine("შეცდომა: თანხა უნდა იყოს დადებითი რიცხვი!");
                }
                else
                {
                    currentAccount.Balance += amount;
                    Console.WriteLine($"თანხა წარმატებით ჩაირიცხა! შეტანილია: {amount} GEL.");
                    currentAccount.TransactionHistory.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FillAmount | თანხა: {amount}");
                    SaveDataToFile();
                    LogAction($"მომხმარებელმა {currentAccount.FullName} შეიტანა {amount} GEL.");
                }
            }
            else
            {
                Console.WriteLine("შეცდომა: გთხოვთ შეიყვანოთ ვალიდური რიცხვი!");
            }
        }

        static void ChangePinCode()
        {
            Console.Write("\nშეიყვანეთ ახალი 4-ნიშნა PIN კოდი: ");
            string newPin = Console.ReadLine()?.Trim();

            if (newPin?.Length == 4 && int.TryParse(newPin, out _))
            {
                currentAccount.PinCode = newPin;
                Console.WriteLine("წარმატებით შეიცვალა PIN კოდი.");
                currentAccount.TransactionHistory.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ChangePin | თანხა: 0");
                SaveDataToFile();
                LogAction($"მომხმარებელმა {currentAccount.FullName} შეცვალა PIN.");
            }
            else
            {
                Console.WriteLine("შეცდომა: პინ კოდი უნდა შედგებოდეს ზუსტად 4 ციფრისგან!");
            }
        }

        static void ConvertCurrency()
        {
            Console.WriteLine($"\nთქვენი ბალანსია: {currentAccount.Balance} GEL");
            Console.WriteLine("აირჩიეთ სამიზნე ვალუტა კონვერტაციისთვის:");
            Console.WriteLine("1. USD (კურსი: 2.80)");
            Console.WriteLine("2. EUR (კურსი: 3.00)");
            Console.Write("არჩევანი (1-2): ");
            string rateChoice = Console.ReadLine()?.Trim();

            double rate = 0;
            string currencyName = "";

            if (rateChoice == "1") { rate = 2.80; currencyName = "USD"; }
            else if (rateChoice == "2") { rate = 3.00; currencyName = "EUR"; }
            else
            {
                Console.WriteLine("არასწორი არჩევანი!");
                return;
            }

            double converted = currentAccount.Balance / rate;
            Console.WriteLine($"თქვენი ბალანსი უცხოურ ვალუტაში შეადგენს: {Math.Round(converted, 2)} {currencyName}");
            currentAccount.TransactionHistory.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CurrencyConversion | კურსი: {rate}");
            LogAction($"მომხმარებელმა {currentAccount.FullName} შეასრულა კონვერტაცია {currencyName}-ში.");
        }
    }
}
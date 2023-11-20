﻿using BankNyBank.Data;
using BankNyBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankNyBank.Utilites.AccountMethods
{
    internal class Withdraw
    {
        public static void WithdrawMenu(BankContext context, User user)
        {
            string pageHeader = $"Please choose account to withdraw from:";

            // Get the user's accounts from the context
            var displayUserAccounts = context.Accounts
                    .Where(u => u.User.Id == user.Id)
                    .Select(a => new
                    {
                        a.Name,
                        Balance = $"{a.Balance:N2}",
                        a.Currency
                    })
                    .ToList();

            // Using an array to store menu options, including user's accounts and "Return to main menu"
            string[] menuOptions = new string[displayUserAccounts.Count + 2];

            // Getting menu options as accounts from current user
            for (int i = 0; i < displayUserAccounts.Count; i++)
            {
                menuOptions[i] = $" {displayUserAccounts[i].Name}";
            }

            // Just to add a space between the listed accounts and "Return to main menu"
            menuOptions[displayUserAccounts.Count] = "";

            // "Return to main menu" option added at the bottom of the options
            menuOptions[displayUserAccounts.Count + 1] = "Return to main menu";

            while (true)
            {
                // To track failed PIN attempts
                int failedAttempts = 0;

                int choice = MenuManager.DisplayAndGetMenuChoice(pageHeader, menuOptions);
                Console.CursorVisible = true;

                if (choice <= displayUserAccounts.Count)
                {
                    string selectedAccountName = displayUserAccounts[choice - 1].Name;
                    Console.WriteLine($"Selected account: {selectedAccountName}");

                    // Get the selected account from the context and display current balance
                    Account selectedAccount = context.Accounts.FirstOrDefault(a => a.User.Id == user.Id && a.Name == selectedAccountName);
                    while (failedAttempts < 3)
                    {
                        Console.Write("Please enter your PIN: ");
                        if (Console.ReadLine() == user.Pin)
                        {
                            Console.WriteLine($"PIN verified.");

                            Console.WriteLine($"Current balance: {selectedAccount.Balance:N2} {displayUserAccounts[choice - 1].Currency}");

                            Console.Write("Please enter withdrawal amount: ");
                            if (double.TryParse(Console.ReadLine(), out double withdrawalAmount) && withdrawalAmount <= selectedAccount.Balance)
                            {
                                // Withdraw funds from the chosen account
                                DbHelper.WithdrawFromAccount(context, selectedAccount, withdrawalAmount);
                                Console.WriteLine($"Withdrawal successful.");
                                Console.WriteLine($"New balance of account {selectedAccount.Name}: {selectedAccount.Balance:N2} {displayUserAccounts[choice - 1].Currency}");
                            }
                            else
                            {
                                Console.WriteLine("Invalid input. Please enter a withdrawal amount within your balance.");
                            }
                            Console.WriteLine("\nPress ENTER to return to main menu.");
                            Console.ReadKey();
                            Console.WriteLine("Returning to the main menu...");
                            Thread.Sleep(1000);
                            return;
                        }
                        else
                        {
                            failedAttempts++;
                            Console.WriteLine($"Incorrect PIN. Attempts left: {3 - failedAttempts}");

                            if (failedAttempts == 3)
                            {
                                Console.WriteLine("Too many incorrect attempts. Logging out...");
                                Thread.Sleep(1000);
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Pin incorrect, try again.");
                            }
                        }
                    }
                }
                else if (choice == displayUserAccounts.Count + 2)
                {
                    Console.WriteLine("Returning to main menu...");
                    Thread.Sleep(1000);
                    return;
                }
            }
        }
    }
}

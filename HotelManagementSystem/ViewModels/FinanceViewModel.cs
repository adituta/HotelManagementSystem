using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Enums;
using System;
using System.Linq;
using System.Data.Entity;

namespace HotelManagementSystem.ViewModels
{
    public class FinanceViewModel : BaseViewModel
    {
        // Proprietăți pentru UI
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalProfit { get { return TotalIncome - TotalExpenses; } }

        public int TotalReservationsCount { get; set; }
        public int TotalMealsServed { get; set; }

        public FinanceViewModel()
        {
            CalculateReports();
        }

        private void CalculateReports()
        {
            using (var db = new HotelDBContext())
            {
                // 1. VENITURI
                // A. Din cazări (sumăm TotalPrice din toate rezervările finalizate sau active)
                decimal roomIncome = db.Reservations.Where(r => r.Status != Enums.ReservationStatus.Cancelled)
                                       .Sum(r => (decimal?)r.TotalPrice) ?? 0;

                // B. Din mâncare (prețul plătit de clienți - DOAR CELE SERVITE)
                decimal foodIncome = db.FoodOrders
                                    .Where(f => f.Status == Enums.OrderStatus.Served)
                                    .Sum(f => (decimal?)f.Cost) ?? 0;

                // C. Din SPA (prețul serviciilor)
                decimal spaIncome = db.SpaAppointments.Sum(s => (decimal?)s.SpaService.PricePerPerson * s.PersonsCount) ?? 0;

                TotalIncome = roomIncome + foodIncome + spaIncome;

                // 2. CHELTUIELI
                // A. Costul alimentelor (InternalCost din MenuItem - DOAR CELE SERVITE)
                decimal foodExpenses = db.FoodOrders
                                      .Where(f => f.Status == Enums.OrderStatus.Served)
                                      .Sum(f => (decimal?)f.MenuItem.InternalCost) ?? 0;

                // B. Salariile personalului SPA (Ore prestate * Salariu/Oră)
                // Presupunem că 1 appointment = 1 oră muncită
                // Facem Join între SpaAppointments și tabelul Employees (dacă ai pus acolo salariul)
                // Pentru simplitate aici, calculăm o medie sau sumăm dintr-un câmp dedicat
                decimal spaSalaries = db.SpaAppointments.Count() * 20; // Exemplu: 20 RON/oră manopera

                // C. Salarii Lunare Fixe (din tabelul SalaryPayments)
                decimal fixedSalaries = db.SalaryPayments.Sum(s => (decimal?)s.Amount) ?? 0;

                TotalExpenses = foodExpenses + spaSalaries + fixedSalaries;

                // 3. STATISTICI
                TotalReservationsCount = db.Reservations.Count();
                TotalMealsServed = db.FoodOrders.Count();

                // Notificăm interfața
                OnPropertyChanged(string.Empty); // Updatează toate proprietățile
            }
        }
    }
}
using HotelManagementSystem.Enums;
using HotelManagementSystem.Helpers;
using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class ReceptionViewModel : BaseViewModel
    {
        private object _currentReceptionView;
        public object CurrentReceptionView
        {
            get => _currentReceptionView;
            set { _currentReceptionView = value; OnPropertyChanged(nameof(CurrentReceptionView)); }
        }

        // Comenzi pentru meniu
        public RelayCommand ShowMapCommand { get; }
        public RelayCommand ShowRequestsCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public ReceptionViewModel()
        {
            // Initializam comenzile
            ShowMapCommand = new RelayCommand(o => CurrentReceptionView = new ReceptionMapViewModel());
            ShowRequestsCommand = new RelayCommand(o => CurrentReceptionView = new ReceptionConfirmationsViewModel());

            // Logout simplu (nu avem referință la MainVM aici, dar putem face un workaround sau lăsa gol momentan 
            // Daca vrei logout corect, trebuie sa primesti MainViewModel in constructor ca la Admin)

            // Pagina de start: Harta (cea pe care o doreai)
            CurrentReceptionView = new ReceptionMapViewModel();
        }
    }
}
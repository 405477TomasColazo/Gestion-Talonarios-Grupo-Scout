using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.Core.Models;
using GestionTalonarios.UI.Helpers;

namespace GestionTalonarios.UI.ViewModels
{
    public class TicketViewModel : ViewModelBase
    {
        private int _code;
        private int _sellerId;
        private string _sellerName;
        private string _sellerSection;
        private int _clientId;
        private string _clientName;
        private string _clientPhone;
        private decimal _unitCost;
        private bool _isPaid;
        private bool _isDelivered;
        private string _observations;
        private DateTime? _withdrawalTime;
        private DateTime _saleDate;
        private DateTime? _paymentDate;
        private decimal _totalAmount; // Monto total (cantidad x precio unitario)
        private int _traditionalQty;
        private int _veganQty;

        public int Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public int SellerId
        {
            get => _sellerId;
            set => SetProperty(ref _sellerId, value);
        }

        public string SellerName
        {
            get => _sellerName;
            set => SetProperty(ref _sellerName, value);
        }

        public string SellerSection
        {
            get => _sellerSection;
            set => SetProperty(ref _sellerSection, value);
        }

        public int ClientId
        {
            get => _clientId;
            set => SetProperty(ref _clientId, value);
        }

        public string ClientName
        {
            get => _clientName;
            set => SetProperty(ref _clientName, value);
        }

        public string ClientPhone
        {
            get => _clientPhone;
            set => SetProperty(ref _clientPhone, value);
        }

        public decimal UnitCost
        {
            get => _unitCost;
            set
            {
                if (SetProperty(ref _unitCost, value))
                {
                    RecalculateTotalAmount();
                }
            }
        }

        

        public int TraditionalQty
        {
            get => _traditionalQty;
            set
            {
                if (SetProperty(ref _traditionalQty, value))
                {
                    RecalculateTotalAmount();
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(QuantityDisplay));
                    OnPropertyChanged(nameof(TraditionalDisplay));
                    OnPropertyChanged(nameof(VeganDisplay));
                }
            }
        }

        public int VeganQty
        {
            get => _veganQty;
            set
            {
                if (SetProperty(ref _veganQty, value))
                {
                    RecalculateTotalAmount();
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(QuantityDisplay));
                    OnPropertyChanged(nameof(TraditionalDisplay));
                    OnPropertyChanged(nameof(VeganDisplay));
                }
            }
        }


        public int Quantity => TraditionalQty + VeganQty;


        public bool IsPaid
        {
            get => _isPaid;
            set => SetProperty(ref _isPaid, value);
        }

        public bool IsDelivered
        {
            get => _isDelivered;
            set => SetProperty(ref _isDelivered, value);
        }

        public string Observations
        {
            get => _observations;
            set => SetProperty(ref _observations, value);
        }

        public DateTime? WithdrawalTime
        {
            get => _withdrawalTime;
            set => SetProperty(ref _withdrawalTime, value);
        }

        public DateTime SaleDate
        {
            get => _saleDate;
            set => SetProperty(ref _saleDate, value);
        }

        public DateTime? PaymentDate
        {
            get => _paymentDate;
            set => SetProperty(ref _paymentDate, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            private set => SetProperty(ref _totalAmount, value);
        }

        // Propiedades adicionales para mostrar en la UI
        public string QuantityDisplay => $"{Quantity} {(Quantity == 1 ? "porción" : "porciones")}";
        public string TraditionalDisplay => $"{TraditionalQty} {(TraditionalQty == 1 ? "tradicional" : "tradicionales")}";
        public string VeganDisplay => $"{VeganQty} {(VeganQty == 1 ? "vegana" : "veganas")}";

        public string TicketNumber => $"#{Code}";
        public string PaymentStatus => IsPaid ? "PAGADO" : "PENDIENTE";
        public string DeliveryStatus => IsDelivered ? "ENTREGADO" : "PENDIENTE DE ENTREGA";

        // Constructor
        public TicketViewModel()
        {
            // Constructor para crear un nuevo ticket
            SaleDate = DateTime.Now;

        }

        // Constructor con mapeo de entidad
        public TicketViewModel(Ticket ticket)
        {
            // Mapear desde la entidad de dominio
            Code = ticket.Code;
            SellerId = ticket.SellerId;
            ClientId = ticket.ClientId;
            UnitCost = ticket.UnitCost;

            IsPaid = ticket.IsPaid;
            IsDelivered = ticket.IsDelivered;
            Observations = ticket.Observations;
            WithdrawalTime = ticket.WithdrawalTime;
            SaleDate = ticket.SaleDate;
            PaymentDate = ticket.PaymentDate;
            TraditionalQty = ticket.TraditionalQty;
            VeganQty = ticket.VeganQty;

            // Mapear propiedades de navegación
            if (ticket.Seller != null)
            {
                SellerName = ticket.Seller.Name;
                SellerSection = ticket.Seller.Section;
            }

            if (ticket.Client != null)
            {
                ClientName = ticket.Client.Name;
                ClientPhone = ticket.Client.Phone;
            }

            RecalculateTotalAmount();
        }

        // Método para recalcular el monto total
        private void RecalculateTotalAmount()
        {
            TotalAmount = UnitCost * (TraditionalQty + VeganQty);
        }

        // Método para convertir el ViewModel a entidad
        public Ticket ToEntity()
        {
            var ticket = new Ticket
            {
                Code = Code,
                SellerId = SellerId,
                ClientId = ClientId,
                UnitCost = UnitCost,
                IsPaid = IsPaid,
                IsDelivered = IsDelivered,
                Observations = Observations,
                WithdrawalTime = WithdrawalTime,
                SaleDate = SaleDate,
                PaymentDate = PaymentDate,
                TraditionalQty = TraditionalQty,
                VeganQty = VeganQty,
            };

            return ticket;
        }
    }
}

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
        private int _id;
        private int _code;
        private int _sellerId;
        private string _sellerName;
        private string _sellerSection;
        private int _clientId;
        private string _clientName;
        private string _clientPhone;
        private decimal _unitCost;
        private int _quantity;
        private bool _isPaid;
        private bool _isDelivered;
        private string _observations;
        private DateTime? _withdrawalTime;
        private DateTime _saleDate;
        private DateTime? _paymentDate;
        private decimal _totalAmount; // Monto total (cantidad x precio unitario)

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

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

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    RecalculateTotalAmount();
                }
            }
        }

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
        public string TicketNumber => $"#{Code}";
        public string PaymentStatus => IsPaid ? "PAGADO" : "PENDIENTE";
        public string DeliveryStatus => IsDelivered ? "ENTREGADO" : "PENDIENTE DE ENTREGA";
        public string QuantityDisplay => $"{Quantity} {(Quantity == 1 ? "porción" : "porciones")}";

        // Constructor
        public TicketViewModel()
        {
            // Constructor para crear un nuevo ticket
            SaleDate = DateTime.Now;
            Quantity = 1;
        }

        // Constructor con mapeo de entidad
        public TicketViewModel(Ticket ticket)
        {
            // Mapear desde la entidad de dominio
            Id = ticket.Id;
            SellerId = ticket.SellerId;
            ClientId = ticket.ClientId;
            UnitCost = ticket.UnitCost;
            Code = ticket.Code;
            Quantity = ticket.Quantity;
            IsPaid = ticket.IsPaid;
            IsDelivered = ticket.IsDelivered;
            Observations = ticket.Observations;
            WithdrawalTime = ticket.WithdrawalTime;
            SaleDate = ticket.SaleDate;
            PaymentDate = ticket.PaymentDate;

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
            TotalAmount = UnitCost * Quantity;
        }

        // Método para convertir el ViewModel a entidad
        public Ticket ToEntity()
        {
            var ticket = new Ticket
            {
                Id = Id,
                SellerId = SellerId,
                ClientId = ClientId,
                Code = Code,
                UnitCost = UnitCost,
                Quantity = Quantity,
                IsPaid = IsPaid,
                IsDelivered = IsDelivered,
                Observations = Observations,
                WithdrawalTime = WithdrawalTime,
                SaleDate = SaleDate,
                PaymentDate = PaymentDate
            };

            return ticket;
        }
    }
}

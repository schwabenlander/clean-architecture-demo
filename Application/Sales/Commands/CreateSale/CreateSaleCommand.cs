using System;
using System.Linq;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Sales.Commands.CreateSale.Factory;

namespace CleanArchitecture.Application.Sales.Commands.CreateSale
{
    public class CreateSaleCommand
        : ICreateSaleCommand
    {
        private readonly TimeProvider _timeProvider;
        private readonly IDatabaseService _database;
        private readonly ISaleFactory _factory;
        private readonly IInventoryService _inventory;

        public CreateSaleCommand(
            TimeProvider timeProvider,
            IDatabaseService database,
            ISaleFactory factory,
            IInventoryService inventory)
        {
            _timeProvider = timeProvider;
            _database = database;
            _factory = factory;
            _inventory = inventory;
        }

        public void Execute(CreateSaleModel model)
        {
            var date = DateTime.SpecifyKind(_timeProvider.GetUtcNow().DateTime.Date, DateTimeKind.Utc);

            var customer = _database.Customers
                .Single(p => p.Id == model.CustomerId);

            var employee = _database.Employees
                .Single(p => p.Id == model.EmployeeId);

            var product = _database.Products
                .Single(p => p.Id == model.ProductId);

            var quantity = model.Quantity;

            var sale = _factory.Create(
                date,
                customer, 
                employee, 
                product, 
                quantity);

            _database.Sales.Add(sale);

            _database.Save();

            _inventory.NotifySaleOccurred(product.Id, quantity);
        }
    }
}

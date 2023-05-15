using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.BerkutPay.Data.Domain;

namespace Nop.Plugin.Payments.BerkutPay.Data.Builder
{
    public class BerkutPayOrderBuilder : NopEntityBuilder<BerkutPayOrder>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(BerkutPayOrder.Id)).AsInt32().PrimaryKey()
                .WithColumn(nameof(BerkutPayOrder.CustomerId)).AsInt32().ForeignKey<Customer>().NotNullable()
                .WithColumn(nameof(BerkutPayOrder.OrderDate)).AsDateTime().NotNullable()
                .WithColumn(nameof(BerkutPayOrder.Amount)).AsDecimal().NotNullable()
                .WithColumn(nameof(BerkutPayOrder.MerchantPacket)).AsString().Nullable()
                .WithColumn(nameof(BerkutPayOrder.BankPacket)).AsString().Nullable()
                .WithColumn(nameof(BerkutPayOrder.Sign)).AsString().Nullable()
                .WithColumn(nameof(BerkutPayOrder.OrderGuid)).AsString().Nullable()
                .WithColumn(nameof(BerkutPayOrder.TransactionResult)).AsString().Nullable();
        }
    }

}

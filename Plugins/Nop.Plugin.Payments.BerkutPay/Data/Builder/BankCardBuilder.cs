using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.BerkutPay.Data.Domain;

namespace Nop.Plugin.Payments.BerkutPay.Data.Builder
{
    public class BankBuilder : NopEntityBuilder<Bank>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(Bank.Id)).AsInt32().PrimaryKey()
                .WithColumn(nameof(Bank.Name)).AsString().NotNullable();
        }
    }

    public class CardBuilder : NopEntityBuilder<Card>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(Card.Id)).AsInt32().PrimaryKey()
                .WithColumn(nameof(Card.PrefixNo)).AsString().NotNullable()
                .WithColumn(nameof(Card.Type)).AsInt32().NotNullable()
                .WithColumn(nameof(Card.IsBusinessCard)).AsBoolean().NotNullable()
                .WithColumn(nameof(Card.BankId)).AsInt32().ForeignKey<Bank>();
        }
    }
}

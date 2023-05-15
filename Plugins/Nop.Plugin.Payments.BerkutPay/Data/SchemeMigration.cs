using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.BerkutPay.Data.Domain;

namespace Nop.Plugin.Payments.BerkutPay.Data
{
    [NopMigration("2023-05-12 10:30:00:0000000", "Bank, Card and BerkutPayOrder Data Migration", MigrationProcessType.Installation)]
    public class SchemeMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<Bank>();

            Create.TableFor<Card>();

            Create.TableFor<BerkutPayOrder>();
        }
    }
}

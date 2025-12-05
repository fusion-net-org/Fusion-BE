using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fusion.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Table_SubscriptionPlanPrice_columnNewPrice_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add limit_unit to UserSubscriptionEntitlements
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'limit_unit' 
                      AND Object_ID = Object_ID('UserSubscriptionEntitlements')
                )
                ALTER TABLE UserSubscriptionEntitlements ADD limit_unit int NULL;
            ");

            // Add monthly_limit to UserSubscriptionEntitlements
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'monthly_limit' 
                      AND Object_ID = Object_ID('UserSubscriptionEntitlements')
                )
                ALTER TABLE UserSubscriptionEntitlements ADD monthly_limit int NULL;
            ");

            // Add auto_grant_monthly to SubscriptionPlans
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'auto_grant_monthly'
                      AND Object_ID = Object_ID('SubscriptionPlans')
                )
                ALTER TABLE SubscriptionPlans ADD auto_grant_monthly bit NOT NULL DEFAULT 0;
            ");

            // Add new_price to SubscriptionPlanPrices (decimal 18,2)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'new_price'
                      AND Object_ID = Object_ID('SubscriptionPlanPrices')
                )
                ALTER TABLE SubscriptionPlanPrices ADD new_price decimal(18,2) NOT NULL DEFAULT 0;
            ");

            // Add monthly_limit to subscriptionplanfeatures
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'monthly_limit'
                      AND Object_ID = Object_ID('subscriptionplanfeatures')
                )
                ALTER TABLE subscriptionplanfeatures ADD monthly_limit int NULL;
            ");

            // Add limit_unit to CompanySubscriptionEntitlements
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'limit_unit'
                      AND Object_ID = Object_ID('CompanySubscriptionEntitlements')
                )
                ALTER TABLE CompanySubscriptionEntitlements ADD limit_unit int NULL;
            ");

            // Add monthly_limit to CompanySubscriptionEntitlements
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'monthly_limit'
                      AND Object_ID = Object_ID('CompanySubscriptionEntitlements')
                )
                ALTER TABLE CompanySubscriptionEntitlements ADD monthly_limit int NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop limit_unit from UserSubscriptionEntitlements
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'limit_unit' 
                      AND Object_ID = Object_ID('UserSubscriptionEntitlements')
                )
                ALTER TABLE UserSubscriptionEntitlements DROP COLUMN limit_unit;
            ");

            // Drop monthly_limit from UserSubscriptionEntitlements
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'monthly_limit' 
                      AND Object_ID = Object_ID('UserSubscriptionEntitlements')
                )
                ALTER TABLE UserSubscriptionEntitlements DROP COLUMN monthly_limit;
            ");

            // Drop auto_grant_monthly from SubscriptionPlans
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'auto_grant_monthly'
                      AND Object_ID = Object_ID('SubscriptionPlans')
                )
                ALTER TABLE SubscriptionPlans DROP COLUMN auto_grant_monthly;
            ");

            // Drop new_price from SubscriptionPlanPrices
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'new_price'
                      AND Object_ID = Object_ID('SubscriptionPlanPrices')
                )
                ALTER TABLE SubscriptionPlanPrices DROP COLUMN new_price;
            ");

            // Drop monthly_limit from subscriptionplanfeatures
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'monthly_limit'
                      AND Object_ID = Object_ID('subscriptionplanfeatures')
                )
                ALTER TABLE subscriptionplanfeatures DROP COLUMN monthly_limit;
            ");

            // Drop limit_unit from CompanySubscriptionEntitlements
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'limit_unit'
                      AND Object_ID = Object_ID('CompanySubscriptionEntitlements')
                )
                ALTER TABLE CompanySubscriptionEntitlements DROP COLUMN limit_unit;
            ");

            // Drop monthly_limit from CompanySubscriptionEntitlements
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'monthly_limit'
                      AND Object_ID = Object_ID('CompanySubscriptionEntitlements')
                )
                ALTER TABLE CompanySubscriptionEntitlements DROP COLUMN monthly_limit;
            ");
        }
    }
}

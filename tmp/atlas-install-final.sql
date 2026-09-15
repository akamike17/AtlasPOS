CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `audit_log` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `user_id` bigint NULL,
        `action` longtext CHARACTER SET utf8mb4 NOT NULL,
        `entity` longtext CHARACTER SET utf8mb4 NOT NULL,
        `entity_id` longtext CHARACTER SET utf8mb4 NULL,
        `detail` longtext CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_audit_log` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `cash_shifts` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `user_id` bigint NOT NULL,
        `opened_at` datetime(6) NOT NULL,
        `closed_at` datetime(6) NULL,
        `opening_amount` decimal(18,2) NOT NULL,
        `expected_amount` decimal(18,2) NULL,
        `counted_amount` decimal(18,2) NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_cash_shifts` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `categories` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `active` tinyint(1) NOT NULL,
        CONSTRAINT `PK_categories` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `customers` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `rfc` longtext CHARACTER SET utf8mb4 NULL,
        `legal_name` longtext CHARACTER SET utf8mb4 NULL,
        `fiscal_regime` longtext CHARACTER SET utf8mb4 NULL,
        `fiscal_zip` longtext CHARACTER SET utf8mb4 NULL,
        `cfdi_use` longtext CHARACTER SET utf8mb4 NULL,
        `email` longtext CHARACTER SET utf8mb4 NULL,
        `phone` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_customers` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `inventory_movements` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `product_id` bigint NOT NULL,
        `user_id` bigint NULL,
        `sale_id` bigint NULL,
        `kind` longtext CHARACTER SET utf8mb4 NOT NULL,
        `quantity` decimal(18,3) NOT NULL,
        `stock_after` decimal(18,3) NOT NULL,
        `note` longtext CHARACTER SET utf8mb4 NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_inventory_movements` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `invoices` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `sale_id` bigint NOT NULL,
        `uuid` longtext CHARACTER SET utf8mb4 NULL,
        `rfc` longtext CHARACTER SET utf8mb4 NOT NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `xml` longblob NULL,
        `pdf` longblob NULL,
        `stamped_at` datetime(6) NULL,
        CONSTRAINT `PK_invoices` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `sale_returns` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `sale_id` bigint NOT NULL,
        `folio` longtext CHARACTER SET utf8mb4 NOT NULL,
        `total` decimal(18,2) NOT NULL,
        `reason` longtext CHARACTER SET utf8mb4 NOT NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_sale_returns` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `signatures` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `sale_id` bigint NOT NULL,
        `device_id` longtext CHARACTER SET utf8mb4 NULL,
        `image` longblob NOT NULL,
        `biometric_data` longblob NULL,
        `captured_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_signatures` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `stores` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `rfc` longtext CHARACTER SET utf8mb4 NULL,
        `timezone` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_stores` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `suppliers` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `rfc` longtext CHARACTER SET utf8mb4 NULL,
        `email` longtext CHARACTER SET utf8mb4 NULL,
        `phone` longtext CHARACTER SET utf8mb4 NULL,
        `active` tinyint(1) NOT NULL,
        CONSTRAINT `PK_suppliers` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `users` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `email` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `password_hash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `role` longtext CHARACTER SET utf8mb4 NOT NULL,
        `active` tinyint(1) NOT NULL,
        CONSTRAINT `PK_users` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `sales` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `shift_id` bigint NOT NULL,
        `customer_id` bigint NULL,
        `folio` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `subtotal` decimal(18,2) NOT NULL,
        `discount` decimal(18,2) NOT NULL,
        `tax` decimal(18,2) NOT NULL,
        `total` decimal(18,2) NOT NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_sales` PRIMARY KEY (`id`),
        CONSTRAINT `FK_sales_cash_shifts_shift_id` FOREIGN KEY (`shift_id`) REFERENCES `cash_shifts` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `products` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `category_id` bigint NULL,
        `sku` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `barcode` varchar(255) CHARACTER SET utf8mb4 NULL,
        `name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `unit` longtext CHARACTER SET utf8mb4 NOT NULL,
        `is_weighted` tinyint(1) NOT NULL,
        `price` decimal(18,2) NOT NULL,
        `cost` decimal(18,4) NOT NULL,
        `tax_rate` decimal(7,4) NOT NULL,
        `stock` decimal(18,3) NOT NULL,
        `minimum_stock` decimal(18,3) NOT NULL,
        `active` tinyint(1) NOT NULL,
        CONSTRAINT `PK_products` PRIMARY KEY (`id`),
        CONSTRAINT `FK_products_categories_category_id` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `purchases` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `supplier_id` bigint NOT NULL,
        `folio` longtext CHARACTER SET utf8mb4 NOT NULL,
        `total` decimal(18,2) NOT NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_purchases` PRIMARY KEY (`id`),
        CONSTRAINT `FK_purchases_suppliers_supplier_id` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `payments` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `sale_id` bigint NOT NULL,
        `method` longtext CHARACTER SET utf8mb4 NOT NULL,
        `amount` decimal(18,2) NOT NULL,
        `reference` longtext CHARACTER SET utf8mb4 NULL,
        `authorization` longtext CHARACTER SET utf8mb4 NULL,
        `provider` longtext CHARACTER SET utf8mb4 NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        `sale_entity_id` bigint NULL,
        CONSTRAINT `PK_payments` PRIMARY KEY (`id`),
        CONSTRAINT `FK_payments_sales_sale_entity_id` FOREIGN KEY (`sale_entity_id`) REFERENCES `sales` (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `sale_lines` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `sale_id` bigint NOT NULL,
        `product_id` bigint NOT NULL,
        `description` longtext CHARACTER SET utf8mb4 NOT NULL,
        `quantity` decimal(18,3) NOT NULL,
        `unit_price` decimal(18,2) NOT NULL,
        `tax` decimal(18,2) NOT NULL,
        `total` decimal(18,2) NOT NULL,
        `sale_entity_id` bigint NULL,
        CONSTRAINT `PK_sale_lines` PRIMARY KEY (`id`),
        CONSTRAINT `FK_sale_lines_sales_sale_entity_id` FOREIGN KEY (`sale_entity_id`) REFERENCES `sales` (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE TABLE `purchase_lines` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `purchase_id` bigint NOT NULL,
        `product_id` bigint NOT NULL,
        `quantity` decimal(18,3) NOT NULL,
        `unit_cost` decimal(18,4) NOT NULL,
        `total` decimal(18,2) NOT NULL,
        `purchase_entity_id` bigint NULL,
        CONSTRAINT `PK_purchase_lines` PRIMARY KEY (`id`),
        CONSTRAINT `FK_purchase_lines_purchases_purchase_entity_id` FOREIGN KEY (`purchase_entity_id`) REFERENCES `purchases` (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE INDEX `IX_payments_sale_entity_id` ON `payments` (`sale_entity_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE INDEX `IX_products_category_id` ON `products` (`category_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE UNIQUE INDEX `IX_products_store_id_barcode` ON `products` (`store_id`, `barcode`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE UNIQUE INDEX `IX_products_store_id_sku` ON `products` (`store_id`, `sku`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE INDEX `IX_purchase_lines_purchase_entity_id` ON `purchase_lines` (`purchase_entity_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE INDEX `IX_purchases_supplier_id` ON `purchases` (`supplier_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE INDEX `IX_sale_lines_sale_entity_id` ON `sale_lines` (`sale_entity_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE INDEX `IX_sales_shift_id` ON `sales` (`shift_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE UNIQUE INDEX `IX_sales_store_id_folio` ON `sales` (`store_id`, `folio`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    CREATE UNIQUE INDEX `IX_users_email` ON `users` (`email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164220_InitialMySqlSchema') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260813164220_InitialMySqlSchema', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `payments` DROP FOREIGN KEY `FK_payments_sales_sale_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `purchase_lines` DROP FOREIGN KEY `FK_purchase_lines_purchases_purchase_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `sale_lines` DROP FOREIGN KEY `FK_sale_lines_sales_sale_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `sale_lines` DROP INDEX `IX_sale_lines_sale_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `purchase_lines` DROP INDEX `IX_purchase_lines_purchase_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `payments` DROP INDEX `IX_payments_sale_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `sale_lines` DROP COLUMN `sale_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `purchase_lines` DROP COLUMN `purchase_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `payments` DROP COLUMN `sale_entity_id`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    CREATE INDEX `IX_sale_lines_sale_id` ON `sale_lines` (`sale_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    CREATE INDEX `IX_purchase_lines_purchase_id` ON `purchase_lines` (`purchase_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    CREATE INDEX `IX_payments_sale_id` ON `payments` (`sale_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `payments` ADD CONSTRAINT `FK_payments_sales_sale_id` FOREIGN KEY (`sale_id`) REFERENCES `sales` (`id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `purchase_lines` ADD CONSTRAINT `FK_purchase_lines_purchases_purchase_id` FOREIGN KEY (`purchase_id`) REFERENCES `purchases` (`id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    ALTER TABLE `sale_lines` ADD CONSTRAINT `FK_sale_lines_sales_sale_id` FOREIGN KEY (`sale_id`) REFERENCES `sales` (`id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813164438_FixEfRelationships') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260813164438_FixEfRelationships', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813180717_AddPeripheralConfigurations') THEN

    CREATE TABLE `peripheral_configurations` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `key` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `provider` longtext CHARACTER SET utf8mb4 NOT NULL,
        `connection_type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `endpoint` longtext CHARACTER SET utf8mb4 NULL,
        `port` int NULL,
        `mode` longtext CHARACTER SET utf8mb4 NULL,
        `device_id` longtext CHARACTER SET utf8mb4 NULL,
        `protected_secret` longtext CHARACTER SET utf8mb4 NULL,
        `enabled` tinyint(1) NOT NULL,
        `last_status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `last_message` longtext CHARACTER SET utf8mb4 NULL,
        `last_tested_at` datetime(6) NULL,
        `updated_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_peripheral_configurations` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813180717_AddPeripheralConfigurations') THEN

    CREATE UNIQUE INDEX `IX_peripheral_configurations_store_id_key` ON `peripheral_configurations` (`store_id`, `key`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813180717_AddPeripheralConfigurations') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260813180717_AddPeripheralConfigurations', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813181417_AddManufacturing') THEN

    CREATE TABLE `production_orders` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `recipe_id` bigint NOT NULL,
        `product_id` bigint NOT NULL,
        `user_id` bigint NOT NULL,
        `folio` longtext CHARACTER SET utf8mb4 NOT NULL,
        `batches` decimal(18,2) NOT NULL,
        `quantity_produced` decimal(18,2) NOT NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_production_orders` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813181417_AddManufacturing') THEN

    CREATE TABLE `recipes` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `product_id` bigint NOT NULL,
        `output_quantity` decimal(18,2) NOT NULL,
        `active` tinyint(1) NOT NULL,
        CONSTRAINT `PK_recipes` PRIMARY KEY (`id`),
        CONSTRAINT `FK_recipes_products_product_id` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813181417_AddManufacturing') THEN

    CREATE TABLE `recipe_lines` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `recipe_id` bigint NOT NULL,
        `product_id` bigint NOT NULL,
        `quantity` decimal(18,3) NOT NULL,
        CONSTRAINT `PK_recipe_lines` PRIMARY KEY (`id`),
        CONSTRAINT `FK_recipe_lines_products_product_id` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE,
        CONSTRAINT `FK_recipe_lines_recipes_recipe_id` FOREIGN KEY (`recipe_id`) REFERENCES `recipes` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813181417_AddManufacturing') THEN

    CREATE INDEX `IX_recipe_lines_product_id` ON `recipe_lines` (`product_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813181417_AddManufacturing') THEN

    CREATE INDEX `IX_recipe_lines_recipe_id` ON `recipe_lines` (`recipe_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813181417_AddManufacturing') THEN

    CREATE INDEX `IX_recipes_product_id` ON `recipes` (`product_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813181417_AddManufacturing') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260813181417_AddManufacturing', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813200826_MultiWorkstationTerminals') THEN

    ALTER TABLE `peripheral_configurations` DROP INDEX `IX_peripheral_configurations_store_id_key`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813200826_MultiWorkstationTerminals') THEN

    ALTER TABLE `sales` ADD `workstation_id` varchar(32) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'SERVER';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813200826_MultiWorkstationTerminals') THEN

    ALTER TABLE `peripheral_configurations` ADD `workstation_id` varchar(32) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'SERVER';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813200826_MultiWorkstationTerminals') THEN

    ALTER TABLE `cash_shifts` ADD `workstation_id` varchar(32) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'SERVER';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813200826_MultiWorkstationTerminals') THEN

    CREATE TABLE `workstations` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `store_id` bigint NOT NULL,
        `terminal_id` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
        `name` longtext CHARACTER SET utf8mb4 NOT NULL,
        `enabled` tinyint(1) NOT NULL,
        `first_seen_at` datetime(6) NOT NULL,
        `last_seen_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_workstations` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813200826_MultiWorkstationTerminals') THEN

    CREATE UNIQUE INDEX `IX_peripheral_configurations_store_id_workstation_id_key` ON `peripheral_configurations` (`store_id`, `workstation_id`, `key`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813200826_MultiWorkstationTerminals') THEN

    CREATE UNIQUE INDEX `IX_workstations_store_id_terminal_id` ON `workstations` (`store_id`, `terminal_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813200826_MultiWorkstationTerminals') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260813200826_MultiWorkstationTerminals', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813201012_SnapshotSyncMultiTerminal') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260813201012_SnapshotSyncMultiTerminal', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813210000_OperationalCashAndIdempotency') THEN

    ALTER TABLE `sales` ADD `client_operation_id` varchar(64) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813210000_OperationalCashAndIdempotency') THEN

    UPDATE sales SET client_operation_id = CONCAT('legacy-', id) WHERE client_operation_id IS NULL

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813210000_OperationalCashAndIdempotency') THEN

    ALTER TABLE `sales` MODIFY COLUMN `client_operation_id` varchar(64) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813210000_OperationalCashAndIdempotency') THEN

    CREATE TABLE `cash_movements` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `shift_id` bigint NOT NULL,
        `user_id` bigint NOT NULL,
        `kind` longtext CHARACTER SET utf8mb4 NOT NULL,
        `amount` decimal(18,2) NOT NULL,
        `reason` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at` datetime(6) NOT NULL,
        CONSTRAINT `PK_cash_movements` PRIMARY KEY (`id`),
        CONSTRAINT `FK_cash_movements_cash_shifts_shift_id` FOREIGN KEY (`shift_id`) REFERENCES `cash_shifts` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813210000_OperationalCashAndIdempotency') THEN

    CREATE UNIQUE INDEX `IX_sales_store_id_client_operation_id` ON `sales` (`store_id`, `client_operation_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813210000_OperationalCashAndIdempotency') THEN

    CREATE INDEX `IX_cash_movements_shift_id` ON `cash_movements` (`shift_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813210000_OperationalCashAndIdempotency') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260813210000_OperationalCashAndIdempotency', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813220000_AddPartialReturnLines') THEN

    CREATE TABLE `sale_return_lines` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `return_id` bigint NOT NULL,
        `sale_line_id` bigint NOT NULL,
        `product_id` bigint NOT NULL,
        `quantity` decimal(18,3) NOT NULL,
        `total` decimal(18,2) NOT NULL,
        `restocked` tinyint(1) NOT NULL,
        CONSTRAINT `PK_sale_return_lines` PRIMARY KEY (`id`),
        CONSTRAINT `FK_sale_return_lines_sale_returns_return_id` FOREIGN KEY (`return_id`) REFERENCES `sale_returns` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813220000_AddPartialReturnLines') THEN

    CREATE INDEX `IX_sale_return_lines_return_id` ON `sale_return_lines` (`return_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813220000_AddPartialReturnLines') THEN

    CREATE INDEX `IX_sale_return_lines_sale_line_id` ON `sale_return_lines` (`sale_line_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260813220000_AddPartialReturnLines') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260813220000_AddPartialReturnLines', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;


# CrossApp_2026
This project models a delivery service where a Customer can create an Order consisting of various Products 
By utilizing OrderLine entries, the system processes the order operations and calculates the total checkout amount

Domain: Orders. Entities: Customer, Product, Order, OrderLine
Purpose: Processing delivery orders and calculating total amounts

## Environment
.NET SDK 10.0, Arch Linux x64

## Інваріанти доменної моделі (Лабораторна 4)

1. **Ідентифікатор замовлення та ім'я клієнта не можуть бути порожніми.** 
   (Перевіряється: `Order.Create`, Кидає: `ArgumentException`).
2. **Ціна товару не може бути від'ємною.** 
   (Перевіряється: `OrderLine.Create`, Кидає: `ArgumentOutOfRangeException`).
3. **Кількість товару в рядку замовлення має бути більшою за нуль.** 
   (Перевіряється: `OrderLine.Create`, Кидає: `ArgumentOutOfRangeException`).
4. **Не можна підтвердити замовлення, якщо воно не містить жодного рядка (порожнє).** 
   (Перевіряється: `Order.Confirm()`, Кидає: `InvalidOperationException`).
5. **Не можна додавати нові рядки до вже підтвердженого замовлення.** 
   (Перевіряється: `Order.AddLine()`, Кидає: `InvalidOperationException`)

## Run
```bash
dotnet build
dotnet run --project src/Cli

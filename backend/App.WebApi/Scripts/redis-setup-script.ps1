# Redis Setup for Multiple Databases
$redisHost = "localhost"
$redisPort = 6379

# Prompt user to choose database
$dbChoice = Read-Host "Enter the database number (0 or 1)"
if ($dbChoice -ne "0" -and $dbChoice -ne "1") {
    Write-Host "Invalid choice. Defaulting to database 0."
    $dbChoice = "0"
}

$redisScript = @"
SELECT $dbChoice
FLUSHDB
SET user_dashboard "{\"RecentOrders\":[{\"OrderId\":1,\"OrderDate\":\"2023-07-28T10:00:00\",\"TotalAmount\":100.00,\"Status\":\"Completed\"}],\"TotalOrders\":1,\"TotalSpent\":100.00}"
SET inventory_status "[{\"Id\":1,\"Name\":\"Test Product\",\"Description\":\"This is a test product\",\"Price\":19.99,\"StockQuantity\":100}]"
GET user_dashboard
GET inventory_status
"@

Write-Host "Setting up Redis on database $dbChoice..."
if (Get-Command "redis-cli" -ErrorAction SilentlyContinue) {
    $redisScript | redis-cli -h $redisHost -p $redisPort
    Write-Host "Redis setup completed successfully on database $dbChoice."
} else {
    Write-Host "redis-cli not found. Please run the following commands manually in your RedisInsight CLI:"
    Write-Host "First, ensure you're connected to the correct database ($dbChoice) on localhost:6379"
    Write-Host $redisScript
}

Write-Host "Redis Setup completed! Please check database $dbChoice for the new keys."
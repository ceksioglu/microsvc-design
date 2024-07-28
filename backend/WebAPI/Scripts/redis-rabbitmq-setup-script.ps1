# Redis Setup
$redisScript = @"
FLUSHALL
SET user_dashboard "{\"RecentOrders\":[{\"OrderId\":1,\"OrderDate\":\"2023-07-28T10:00:00\",\"TotalAmount\":100.00,\"Status\":\"Completed\"}],\"TotalOrders\":1,\"TotalSpent\":100.00}"
SET inventory_status "[{\"Id\":1,\"Name\":\"Test Product\",\"Description\":\"This is a test product\",\"Price\":19.99,\"StockQuantity\":100}]"
GET user_dashboard
GET inventory_status
"@

Write-Host "Setting up Redis..."
if (Get-Command "redis-cli" -ErrorAction SilentlyContinue) {
    $redisScript | redis-cli
    Write-Host "Redis setup completed successfully."
} else {
    Write-Host "redis-cli not found. Please run the following commands manually in your Redis CLI:"
    Write-Host $redisScript
}

# RabbitMQ Setup
$rabbitMQHost = "localhost:15672"
$rabbitMQUser = "guest"
$rabbitMQPass = "guest"

$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $rabbitMQUser, $rabbitMQPass)))

$queues = @("order_created", "feedback_submitted")

Write-Host "Setting up RabbitMQ..."
foreach ($queue in $queues) {
    $body = @{
        name = $queue
        durable = $false
        auto_delete = $false
        arguments = @{}
    } | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri "http://$rabbitMQHost/api/queues/%2F/$queue" -Method Put -Body $body -ContentType "application/json" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        Write-Host "Queue '$queue' created or already exists."
    } catch {
        Write-Host "Failed to create queue '$queue'. Error: $_"
    }
}

Write-Host "Setup completed!"

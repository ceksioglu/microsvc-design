# microsvc-design
 Microservice Design Project

graph TD
    A[React Frontend] -->|HTTP| B[API Gateway]
    B -->|HTTP| C[Command Service]
    B -->|HTTP| D[Query Service]
    C -->|Publish Events| E[RabbitMQ]
    E -->|Consume Events| F[Saga Orchestrator]
    F -->|Publish Commands| E
    C -->|Write| G[(Postgres)]
    D -->|Read| H[(Redis)]
    I[Event Handler] -->|Update| H
    E -->|Consume Events| I

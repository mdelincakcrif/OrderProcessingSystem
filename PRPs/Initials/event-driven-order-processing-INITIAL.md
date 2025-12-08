## FEATURE:

Common requirements:
 - Use messaging service RabbitMq. Update docker compose file so this service can be created inside docker.
 - Add event bus into the project so the messages/events can be sent/published. As the transport system for event bus use the chosen messaging service.

Order creation handling:
 - When the order is created the OrderCreated event has to be published
 - There will be handling of this event which:
   - Update order status: pending -> processing
   - Simulate payment processing (5 second delay)
   - Update order status for 50% of cases to completed and publish OrderCompleted event
   - In another 50% of cases do not change the status

Order expiration handling:
 - Add recursive job which will run every 60 seconds
 - The job find orders with status='processing' older than 10 minutes and update the status to 'expired'
 - Publish OrderExpired event

Notifications handling
 - Create new notifications table and add upgrade script/code
 - When the OrderCompleted event is published
   - Send email notification (for now mock it just log to console)
   - Save notification to database (audit trail)
 - When the OrderExpired event is published
   - Save notification to database (audit trail)

Expected Flow:
1. User creates order via POST /api/orders
2. Order saved to DB with status='pending'
3. OrderCreated event published
4. OrderProcessor handles event asynchronously:
  - Updates status to 'processing'
  - Simulates payment (5 sec delay)
  - Updates status to 'completed'
5. OrderCompleted event published
6. Notifier handles event:
  - Logs fake email to console
  - Saves notification to DB
7. CRON job runs every 60s:
  - Finds pending orders in status processing older than 10 minutes
  - Updates them to 'expired'

Additional requirements
 - Add tests for added functionality
 - all ids are GUIDs
 - every date is in UTC

## EXAMPLES:


## DOCUMENTATION:


## OTHER CONSIDERATIONS:
 - move the job logic into separated project called Worker
 - enhance docker compose to run also WebApi, Worker, RabbitMq and CRON job
 - follow the existing structure and domain driven design
 - test will be implemented using xUnit and FluentAssertions and if needed for moqs use NSubstitute
 - for tests use in memory database provider for EF Core
 - focus on clean code and SOLID principles, YAGNI principle, KISS principle and functional requirements
 - update CLAUDE.md file with any new information you find important
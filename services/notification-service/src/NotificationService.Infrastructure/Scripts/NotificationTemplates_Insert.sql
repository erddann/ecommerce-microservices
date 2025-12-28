-- NotificationTemplates Insert Script
-- Run this script after creating the database to populate initial templates

INSERT INTO NotificationTemplates (Id, TemplateCode, Channel, Language, Subject, Body, IsActive, Version)
VALUES
    ('550e8400-e29b-41d4-a716-446655440000', 'ORDER_CONFIRMED', 1, 'en', 'Order Confirmation', 'Dear Customer,\n\nYour order {OrderNumber} has been confirmed successfully.\n\nOrder Total: {OrderTotal}\n\nThank you for shopping with us!', 1, 1),
    ('550e8400-e29b-41d4-a716-446655440001', 'ORDER_CANCELLED', 1, 'en', 'Order Cancellation', 'Dear Customer,\n\nWe regret to inform you that your order {OrderId} has been cancelled.\n\nReason: {Reason}\n\nPlease contact support if you have any questions.', 1, 1);

-- Note: Channel 1 = Email, 2 = Sms
-- Ids are GUIDs, adjust as needed
-- Language codes: 'en' for English, add more as required
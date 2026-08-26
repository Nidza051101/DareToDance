# SmsChannel

Prazan skelet — kasnije, van obima ove faze (v. artifact "Notification gRPC Flow").

Kad dođe na red, prati isti obrazac kao `Features/EmailChannel/`:
`Commands/SendSmsViaXxx/` + sopstveni `Shared/` (npr. provajder-specifične
postavke). `NotificationQueueConsumer` već ima `case NotificationChannel.Sms`
pripremljen, samo baca `ChannelNotImplemented` upozorenje za sada.

﻿using AuthService.Models;
using AuthService.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;
using System.Text.Json;

namespace AuthService.ServiceBus;
public class AccountCreatedMessageHandler : BackgroundService
    //Claude AI generated the base for this handler and I have modified it some. 
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<AccountCreatedMessageHandler> _logger;

    public AccountCreatedMessageHandler(ServiceBusClient serviceBusClient, IServiceProvider serviceProvider, ILogger<AccountCreatedMessageHandler> logger)
    {
        _serviceBusClient = serviceBusClient;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _processor = serviceBusClient.CreateProcessor("account-created", "email");
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var messageBody = args.Message.Body.ToString();
            var baseEvent = JsonSerializer.Deserialize<JsonElement>(messageBody);
            var eventType = baseEvent.GetProperty("EventType").GetString(); 

            if (eventType == "EmailVerified")
            {
                var emailVerifiedEvent = JsonSerializer.Deserialize<EmailVerifiedEvent>(messageBody);

                using var scope = _serviceProvider.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                var result = await userService.ConfirmUser(emailVerifiedEvent!.Email);
                if (result != null)
                {
                    await PublishUserConfirmedEvent(result);
                }
            }

            await args.CompleteMessageAsync(args.Message); 
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error processing user event message.");
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processing error");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken); 
    }

    private async Task PublishUserConfirmedEvent(UserEntity user)
    {
        var sender = _serviceBusClient.CreateSender("account-created");
        var eventMessage = new UserConfirmedEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        var message = new ServiceBusMessage(JsonSerializer.Serialize(eventMessage));

        await sender.SendMessageAsync(message);
    }
}


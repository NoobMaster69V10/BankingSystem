﻿using BankingSystem.Core.DTO.Result;
using BankingSystem.Domain.Entities;
using BankingSystem.Core.ServiceContracts;
using BankingSystem.Domain.Errors;
using BankingSystem.Domain.UnitOfWorkContracts;
using BankingSystem.Core.DTO.BankAccount;

namespace BankingSystem.Core.Services;

public class BankAccountService(IUnitOfWork unitOfWork, ILoggerService loggerService) : IBankAccountService
{
    public async Task<Result<BankAccount>> CreateBankAccountAsync(BankAccountRegisterDto bankAccountRegisterDto, CancellationToken cancellationToken)
    {
        try
        {
            var bankAccount = await unitOfWork.BankAccountRepository.GetAccountByIbanAsync(bankAccountRegisterDto.Iban, cancellationToken);
            if (bankAccount != null)
            {
                return Result<BankAccount>.Failure(CustomError.Validation("Bank account already exists."));
            }

            var person = await unitOfWork.PersonRepository.GetByUsernameAsync(bankAccountRegisterDto.Username, cancellationToken);
            if (person == null)
            {
                return Result<BankAccount>.Failure(CustomError.NotFound("User not found."));
            }

            var account = new BankAccount
            {
                Iban = bankAccountRegisterDto.Iban,
                Balance = bankAccountRegisterDto.Balance,
                PersonId = person.PersonId,
                Currency = bankAccountRegisterDto.Currency
            };

            await unitOfWork.BankAccountRepository.AddBankAccountAsync(account, cancellationToken);

            return Result<BankAccount>.Success(account);
        }
        catch (Exception ex)
        {
            loggerService.LogError($"Error in CreateBankAccountAsync: {ex}");
            return Result<BankAccount>.Failure(CustomError.Failure("Account could not be created."));
        }
    }
}
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Contract.Requests;
using Fusion.Service.ViewModels.Contract.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IContractAppendixRepository _contractAppendixRepository;

        public ContractService(IContractRepository contractRepository, IContractAppendixRepository contractAppendixRepository)
        {
            _contractRepository = contractRepository;
            _contractAppendixRepository = contractAppendixRepository;
        }

        public async Task<ContractResponse> CreateContractAsync(Guid userId, CreateContractRequest request, CancellationToken ct = default)
        {
            var contract = await _contractRepository.CreateContractAsync(userId, request.ProjectRequestId, new Contract
            {
                ContractCode = request.ContractCode,
                ContractName = request.ContractName,
                Budget = request.Budget,
                EffectiveDate = request.EffectiveDate,
                ExpiredDate = request.ExpiredDate,
            }, ct);

            if (contract == null)
            {
                throw CustomExceptionFactory.CreateInternalServerError("Create CO");
            }

            var appendices = await _contractAppendixRepository.CreateContractAppendixAsync(contract.Id, request.Appendices, ct);

            var response = new ContractResponse
            {
                Id = contract.Id,
                ContractCode = contract.ContractCode,
                ContractName = contract.ContractName,
                Budget = contract.Budget.Value,
                EffectiveDate = contract.EffectiveDate.Value,
                ExpiredDate = contract.ExpiredDate.Value,
                ProjectRequestId = contract.ProjectRequestId,

                Appendices = appendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode,
                }).ToList()
            };

            return response;
        }

        public async Task<ContractResponse> UpdateContractAsync(Guid contractId, Guid userId, UpdateContractRequest request, CancellationToken ct = default)
        {
            var contract = await _contractRepository.UpdateContractAsync(contractId, userId, new Contract
            {
                ContractCode = request.ContractCode,
                ContractName = request.ContractName,
                Budget = request.Budget,
                EffectiveDate = request.EffectiveDate,
                ExpiredDate = request.ExpiredDate,
            }, request.Appendices, ct);

            var response = new ContractResponse
            {
                Id = contract.Id,
                ContractCode = contract.ContractCode,
                ContractName = contract.ContractName,
                Budget = contract.Budget.Value,
                EffectiveDate = contract.EffectiveDate.Value,
                ExpiredDate = contract.ExpiredDate.Value,
                ProjectRequestId = contract.ProjectRequestId,

                Appendices = contract.ContractAppendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode,
                }).ToList()
            };

            return response;

        }

        public async Task<ContractResponse> GetContractByIdAsync(Guid contractId, CancellationToken ct = default)
        {
            var contract = await _contractRepository.GetContractByIdAsync(contractId, ct);

            var response = new ContractResponse
            {
                Id = contract.Id,
                ContractCode = contract.ContractCode,
                ContractName = contract.ContractName,
                Budget = contract.Budget.Value,
                EffectiveDate = contract.EffectiveDate.Value,
                ExpiredDate = contract.ExpiredDate.Value,
                ProjectRequestId = contract.ProjectRequestId,

                Appendices = contract.ContractAppendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode,
                }).ToList()
            };

            return response;
        }

        public async Task<List<ContractResponse>> GetAllContractsAsync(CancellationToken ct = default)
        {
            var contracts = await _contractRepository.GetAllContractsAsync(ct);

            if (contracts == null || !contracts.Any())
                return new List<ContractResponse>();

            var response = contracts.Select(contract => new ContractResponse
            {
                Id = contract.Id,
                ContractCode = contract.ContractCode,
                ContractName = contract.ContractName,
                Budget = contract.Budget.Value,
                EffectiveDate = contract.EffectiveDate.Value,
                ExpiredDate = contract.ExpiredDate.Value,
                ProjectRequestId = contract.ProjectRequestId,

                Appendices = contract.ContractAppendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode,
                }).ToList()

            }).ToList();

            return response;
        }
    }
}
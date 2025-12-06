using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Contract;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Entities;
using Fusion.Repository.Enums;
using Fusion.Repository.IRepositories;
using Fusion.Repository.Repositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Contract.Requests;
using Fusion.Service.ViewModels.Contract.Responses;
using Microsoft.AspNetCore.Http;
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
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IProjectRequestRepository _projectRequestRepo;
        public ContractService(IContractRepository contractRepository, IContractAppendixRepository contractAppendixRepository, ICloudinaryService cloudinaryService, IProjectRequestRepository projectRequestRepo)
        {
            _contractRepository = contractRepository;
            _contractAppendixRepository = contractAppendixRepository;
            _cloudinaryService = cloudinaryService;
            _projectRequestRepo = projectRequestRepo;
        }

        public async Task<string> UploadContractAttachmentAsync(Guid contractId, IFormFile file, Guid userId, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty.", nameof(file));

            var (url, publicId) = await _cloudinaryService.UploadDocumentAsync(
                  file,
                  "ContractAttachment",  // folder
                  ct
              );

            await _contractRepository.UpdateContractAttachmentAsync(contractId,url,userId,ct);

            return url;
        }

        public async Task<ContractResponse> CreateContractAsync(Guid userId, CreateContractRequest request, CancellationToken ct = default)
        {
            //string? attachmentUrl = null;

            //if (request.AttachmentFile != null)
            //{
            //    attachmentUrl = await _cloudinaryService.UploadImageAsync(
            //        request.AttachmentFile,
            //        "ContractAttachment",
            //        ct
            //    );
            //}
            if (request.Budget < 0)
                throw CustomExceptionFactory.CreateBadRequestError("Budget cannot be negative.");

            if (request.EffectiveDate >= request.ExpiredDate)
                throw CustomExceptionFactory.CreateBadRequestError("EffectiveDate must be earlier than ExpiredDate.");


            var contract = await _contractRepository.CreateContractAsync(userId, new Contract
            {
                ContractCode = request.ContractCode,
                ContractName = request.ContractName,
                Budget = request.Budget,
                EffectiveDate = request.EffectiveDate,
                ExpiredDate = request.ExpiredDate,
                Status = "DRAFT",
                ExecutorCompanyId = request.ExecutorCompanyId,
                RequesterCompanyId = request.RequesterCompanyId
                //Attachment = attachmentUrl,
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
                Status = contract.Status,
                ExecutorCompanyId = (Guid)contract.ExecutorCompanyId,
                RequesterCompanyId = (Guid)contract.RequesterCompanyId,

                Appendices = appendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode,
                }).ToList(),
                Attachment = contract.Attachment
            };

            return response;
        }

        public async Task<ContractResponse> UpdateContractAsync(
            Guid contractId,
            Guid userId,
            UpdateContractRequest request,
            CancellationToken ct = default)
        {
            var contract = await _contractRepository.UpdateContractAsync(
                contractId,
                userId,
                new Contract
                {
                    ContractCode = request.ContractCode,
                    ContractName = request.ContractName,
                    Budget = request.Budget,
                    EffectiveDate = request.EffectiveDate,
                    ExpiredDate = request.ExpiredDate,
                },
                request.Appendices,
                ct
            );

            contract.Status = "PENDING";
            await _contractRepository.UpdateContractStatusAsync(contractId, userId, "Pending", ct);

            var projectRequest = await _projectRequestRepo.GetProjectRequestByContractIdAsync(contractId, ct);
            if (projectRequest != null)
            {
                ProjectRequestStatusEnum.Pending.ToString();
                await _projectRequestRepo.UpdateProjectRequestStatusAsync(projectRequest.Id, ProjectRequestStatusEnum.Pending, ct);
            }

            var response = new ContractResponse
            {
                Id = contract.Id,
                ContractCode = contract.ContractCode,
                ContractName = contract.ContractName,
                Budget = contract.Budget ?? 0,
                EffectiveDate = contract.EffectiveDate ?? DateOnly.MinValue,
                ExpiredDate = contract.ExpiredDate ?? DateOnly.MinValue,
                Status = contract.Status,
                Appendices = contract.ContractAppendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode,
                    AppendixDescription = a.Description
                }).OrderBy(a => a.AppendixCode).ToList()
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
                Status = contract.Status,
                Appendices = contract.ContractAppendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode,
                    AppendixDescription = a.Description,
                }).ToList(),
                Attachment = contract.Attachment,
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

                Appendices = contract.ContractAppendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode,
                }).ToList()

            }).ToList();

            return response;
        }
        public async Task<ContractResponse> UpdateContractStatusAsync(Guid contractId, Guid userId, string status, CancellationToken ct = default)
        {
            var contract = await _contractRepository.UpdateContractStatusAsync(contractId, userId, status, ct);

            return new ContractResponse
            {
                Id = contract.Id,
                ContractCode = contract.ContractCode,
                ContractName = contract.ContractName,
                Budget = contract.Budget ?? 0,
                EffectiveDate = contract.EffectiveDate ?? DateOnly.MinValue,
                ExpiredDate = contract.ExpiredDate ?? DateOnly.MinValue,
                Status = contract.Status,
                Appendices = contract.ContractAppendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode
                }).ToList()
            };
        }

        public async Task<bool> ContractExistsAsync(Guid contractId, CancellationToken ct)
        {
            return await _contractRepository.ContractExistsAsync(contractId, ct);
        }

        public async Task<PagedResult<ContractResponse>> GetAllContractsAdminAsync(ContractSearchRequest request,
    CancellationToken ct = default)
        {
            var pagedContracts = await _contractRepository.GetAllContractsAdminAsync(request, ct);

            if (pagedContracts == null || pagedContracts.Items.Count == 0)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Contract"));

            var responseItems = pagedContracts.Items.Select(contract => new ContractResponse
            {
                Id = contract.Id,
                ContractCode = contract.ContractCode,
                ContractName = contract.ContractName,
                Budget = contract.Budget ?? 0,
                Status = contract.Status ?? "Unknown",
                EffectiveDate = contract.EffectiveDate ?? default,
                ExpiredDate = contract.ExpiredDate ?? default,

                Appendices = contract.ContractAppendices.Select(a => new ContractAppendixResponse
                {
                    Id = a.Id,
                    AppendixName = a.Title,
                    AppendixCode = a.AppendixCode
                }).ToList()

            }).ToList();

            return new PagedResult<ContractResponse>
            {
                Items = responseItems,
                TotalCount = pagedContracts.TotalCount,
                PageNumber = pagedContracts.PageNumber,
                PageSize = pagedContracts.PageSize
            };
        }

    }
}
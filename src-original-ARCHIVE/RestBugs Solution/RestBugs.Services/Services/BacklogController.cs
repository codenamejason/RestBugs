﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using RestBugs.Services.Model;

namespace RestBugs.Services.Services
{
    public class BacklogController : ApiController
    {
        readonly IBugRepository _bugsRepository;

        public BacklogController(IBugRepository bugsRepository) {
            _bugsRepository = bugsRepository;
        }

        public HttpResponseMessage Get() {
            var response = Request.CreateResponse(HttpStatusCode.OK, BugModel.Collection(GetBacklogBugDtos()));

            var n = Configuration.Services.GetContentNegotiator();
            n.Negotiate(typeof(BugModel), Request, Configuration.Formatters);

            return response;
        }

        private IEnumerable<BugDTO> GetBacklogBugDtos()
        {
            var bugs = _bugsRepository.GetAll().Where(b => b.Status == BugStatus.Backlog);
            var dtos = Mapper.Map<IEnumerable<Bug>, IEnumerable<BugDTO>>(bugs);
            return dtos;
        }

        public HttpResponseMessage Post (BugDTO dto, string comments=null)
        {
            Bug bug;
            if(dto.Id != 0)
            {
                bug = _bugsRepository.Get(dto.Id);
                if (bug == null)
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                bug.Approve();
                return Request.CreateResponse(HttpStatusCode.OK, BugModel.Collection(GetBacklogBugDtos()));
            }

            bug = Mapper.Map<BugDTO, Bug>(dto);
            bug.Approve();

            _bugsRepository.Add(bug);
            
            var response = Request.CreateResponse(HttpStatusCode.Created, BugModel.Collection(GetBacklogBugDtos()));
            
            //i still don't like this because it's fragmenting management of my links
            response.Headers.Location = new Uri(HostUriFromRequest(Request), bug.Id.ToString(CultureInfo.InvariantCulture));

            return response;
        }

        static Uri HostUriFromRequest(HttpRequestMessage requestMessage)
        {
            return new Uri(String.Format("{0}://{1}:{2}",
                                         requestMessage.RequestUri.Scheme,
                                         requestMessage.RequestUri.Host,
                                         requestMessage.RequestUri.Port));
        }
    }
}
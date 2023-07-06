using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using BriefLynxNew.BriefLynx.DataAccess.Interfaces;
using BriefLynxNew.BriefLynx.DataAccess.Model;
using BriefLynxNew.BriefLynx.DataAccess.Enumerations;
using System.Linq.Expressions;
using System.IO;
using System.Text.RegularExpressions;
using BriefLynxNew.BriefLynx.DataAccess.Exceptions;
using System.Globalization;
using BriefLynxNew.BriefLynx.Common;

namespace BriefLynxNew.BriefLynx.DataAccess
{
    /// <summary>
    /// Brief Repository
    /// </summary>
	public class BriefRepository : BriefLynxRepository<Brief>, IBriefRepository
    {

        private static readonly Expression<Func<Brief, bool>> BriefIsLockedExpression =
            b => b.PassPhrase != null && b.PassPhrase != string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="BriefRepository"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public BriefRepository(BriefLynxDataContext dataContext, DataContextFactory dataContextFactory)
            : base(dataContext, dataContextFactory)
        { }

        /// <summary>
        /// Gets the brief.
        /// </summary>
        /// <param name="briefId">The brief id.</param>
        /// <returns></returns>
		public Brief GetBrief(int briefId)
        {
            return DataContext.Briefs.SingleOrDefault(b => b.BriefID == briefId);
        }

        /// <summary>
        /// Gets the entity table.
        /// </summary>
        /// <value>
        /// The entity table.
        /// </value>
		protected override Table<Brief> EntityTable
        {
            get { return DataContext.Briefs; }
        }

        /// <summary>
        /// Gets the current version of the specified brief from
        /// persistent storage
        /// </summary>
        /// <param name="briefId">The identifier of the brief to retrieve</param>
        /// <returns></returns>
        public PackageStatusEnumeration GetBriefPackageStatus(int briefId)
        {
            using (var context = DataContextFactory.CreateNewContext())
            {
                Brief brief = (from b in context.Briefs where b.BriefID == briefId select b).SingleOrDefault();

                int? status = brief.ZipFileCreationStatusID;

                if (status.HasValue)
                {
                    return (PackageStatusEnumeration)status.Value;
                }
            }

            return PackageStatusEnumeration.Unknown;
        }

        /// <summary>
        /// Fetch the annotated master document for a brief.
        /// </summary>
        /// <param name="briefId"></param>
        public IQueryable<BriefDocument> GetAnnotatedMasterDocument(int briefId)
        {
            return from document in DataContext.Documents.OfType<BriefDocument>()
                   where
                       document.BriefID == briefId &&
                       document.DocumentTypeID == (int)DocumentTypeEnumeration.AnnotatedMaster
                   orderby document.FileName
                   select document;
        }

        /// <summary>
        /// Lists all master documents for the specified brief.
        /// </summary>
        /// <param name="briefID">The brief ID.</param>
        /// <returns></returns>
        public IQueryable<BriefDocument> ListBriefMasterDocuments(int briefID)
        {
            return (from document in DataContext.Documents.OfType<BriefDocument>()
                    where document.BriefID == briefID && document.DocumentTypeID == (int)DocumentTypeEnumeration.Master
                    orderby document.FileName
                    select document);
        }

        /// <summary>
        /// Lists all registered documents within a brief
        /// </summary>
        /// <param name="briefID">The brief ID.</param>
        /// <param name="refreshFromDatabase">if set to <c>true</c> [refresh from database].</param>
        /// <returns></returns>
        public IQueryable<BriefDocument> ListBriefRegisteredDocuments(int briefID, bool refreshFromDatabase)
        {
            int matterID = GetBrief(briefID).MatterID;

            var lockedBriefs = DataContext.Briefs.Where(Brief.IsReadOnlyExpression).ToList();

            var documents = from document in DataContext.Documents.OfType<BriefDocument>()
                            where
                                (document.BriefID == briefID &&
                                 document.DocumentTypeID == (int)DocumentTypeEnumeration.Attachment) ||
                                ((from b in lockedBriefs where b.MatterID == matterID select b.BriefID).Contains(
                                     document.BriefID) &&
                                 document.DocumentTypeID == (int)DocumentTypeEnumeration.Master)
                            orderby document.FileName
                            select document;

            if (refreshFromDatabase)
            {
                Refresh(documents, RefreshMode.OverwriteCurrentValues);
            }

            return documents;
        }

        /// <summary>
        /// Lists the enabled briefs.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Brief> ListEnabledBriefs()
        {
            return from brief in DataContext.Briefs
                   where brief.Enabled
                   orderby brief.Matter.Name, brief.Name
                   select brief;
        }

        /// <summary>
        /// Lists the read only briefs for the given user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public IQueryable<Brief> ListReadOnlyBriefs(int userId)
        {
            return from b in DataContext.Briefs
                   where
                       b.BriefViewers.Select(bv => bv.UserID).Contains(userId) && b.Enabled &&
                       Brief.GeneratedPackageStatuses.Cast<int>().Contains(b.ZipFileCreationStatusID ??
                                                                           (int)
                                                                           PackageStatusEnumeration.NotGenerated)
                   select b;
        }

        public void DeleteBriefViewer(BriefViewer viewer)
        {
            if (viewer == null)
                throw new ArgumentNullException("viewer");

            DataContext.BriefViewers.DeleteOnSubmit(viewer);
        }

        /// <summary>
        /// Lists all index image documents for the specified brief.
        /// </summary>
        /// <param name="briefID">The brief ID.</param>
        /// <returns></returns>
        public IQueryable<BriefDocument> ListBriefImageDocuments(int briefID)
        {
            return from document in DataContext.Documents.OfType<BriefDocument>()
                   where document.BriefID == briefID && document.DocumentTypeID == (int)DocumentTypeEnumeration.Image
                   orderby document.FileName
                   select document;
        }

        /// <summary>
        /// Creates a new brief review
        /// </summary>
        /// <param name="brief">The brief</param>
        /// <param name="reviewer">The reviewer</param>
        /// <returns></returns>
    	public BriefReview NewBriefReview(Brief brief, User reviewer)
        {
            var briefReview = new BriefReview()
            {
                Brief = brief,
                Reviewer = reviewer,
                DateAssigned = DateTime.Now
            };

            DataContext.BriefReviews.InsertOnSubmit(briefReview);

            return briefReview;
        }
        /// <summary>
        /// Sets the name of the master document.
        /// </summary>
        /// <param name="briefId">The brief id.</param>
        /// <param name="masterDocName">Name of the master doc.</param>        
        public void SetMasterDocumentName(int briefId, string masterDocName)
        {
            var brief = GetBrief(briefId);
            // If the user specified name is not the same as the original name, check it and set it
            if (masterDocName != Path.GetFileNameWithoutExtension(brief.MasterDocument.FileName))
            {
                var docName = new Regex(@"^[a-zA-Z0-9,'_.\s\-]+$");

                if (!docName.IsMatch(masterDocName))
                {
                    throw new ArgumentException("Invalid file name.", masterDocName);
                }

                var fileNames =
                    (from Document doc in ListBriefValidDestinationDocuments(briefId) select Path.GetFileNameWithoutExtension(doc.FileName)).ToList();

                fileNames.AddRange(from Document doc in ListBriefImageDocuments(briefId) select Path.GetFileNameWithoutExtension(doc.FileName));

                //Check for duplicate file names
                var duplicates = fileNames.Where(name => masterDocName == name).ToList();

                if (duplicates.Any())
                {
                    throw new DuplicateDocumentNameException(duplicates);
                }

                // If we've made it this far, the new name should be valid
                brief.MasterDocument.PackageFileName = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}",
                    masterDocName,
                    !masterDocName.EndsWith(brief.MasterDocument.FileExtension, StringComparison.OrdinalIgnoreCase)
                        ? brief.MasterDocument.FileExtension
                        : string.Empty);

                CommitAllChanges();
            }
        }

        /// <summary>
        /// Updates the status of the package generation
        /// </summary>
        /// <param name="briefID">Brief ID</param>
        /// <param name="status">Status to update</param>
        public void UpdatePackageGenerationStatus(int briefID, PackageStatusEnumeration status)
        {
            bool statusChanged = false;

            using (var dataContext = DataContextFactory.CreateNewContext())
            {
                Brief brief = (from b in dataContext.Briefs where b.BriefID == briefID select b).SingleOrDefault();

                if (brief != null && brief.ZipFileCreationStatusID != (int)status)
                {
                    brief.ZipFileCreationStatusID = (int)status;

                    dataContext.SubmitChanges();

                    statusChanged = true;
                }
            }

            // Brief Performance Monitors
            if (statusChanged && (status == PackageStatusEnumeration.Error
                                    || status == PackageStatusEnumeration.Draft
                                    || status == PackageStatusEnumeration.Generated
                                    || status == PackageStatusEnumeration.Aborted))
            {   // Update package complete performance counters
                CustomPerformanceCounters.IncrementBriefPackageGenerated(status == PackageStatusEnumeration.Draft || status == PackageStatusEnumeration.Generated);
            }
        }

        public Brief NewBrief(User owner)
        {
            var now = DateTime.Now;

            var brief = new Brief
            {
                Creator = owner,
                CreateDate = now,
                LastModifiedDate = now,
                ZipFileCreationStatusID = (int)PackageStatusEnumeration.NotGenerated
            };

            DataContext.Briefs.InsertOnSubmit(brief);

            return brief;
        }

        /// <summary>
        /// Lists all master documents and destination documents including destinations from
        /// related briefs
        /// </summary>
        /// <param name="briefID">The Brief ID</param>
        /// <returns></returns>
        public IQueryable<BriefDocument> ListBriefValidDestinationDocuments(int briefID)
        {
            var brief = GetBrief(briefID);

            var documents = (from d in DataContext.Documents.OfType<BriefDocument>()
                             where
                                 d.BriefID == briefID &&
                                 (d.DocumentTypeID == (int)DocumentTypeEnumeration.Master ||
                                  d.DocumentTypeID == (int)DocumentTypeEnumeration.Attachment)
                             select d).Union(from b in DataContext.Briefs.Where(BriefIsLockedExpression)
                                             join d in DataContext.Documents.OfType<BriefDocument>() on b.BriefID equals d.BriefID
                                             where
                                                 b.MatterID == brief.MatterID &&
                                                 d.DocumentTypeID == (int)DocumentTypeEnumeration.Master
                                             select d);

            return documents;
        }

        public IQueryable<AllExhibitByMatterList> GetExhibitListByMatter(User user, int matterid)
        {
            IQueryable<AllExhibitByMatterList> exhibitLists = null;
            if ((RoleEnumeration)user.UserRoles.Single().RoleID == RoleEnumeration.Administrator)
            {
                exhibitLists = (from a in DataContext.ExhibitLists
                                join b in DataContext.Exhibits on a.Id equals b.ExhibitListId
                                where a.MatterId == matterid
                                select new AllExhibitByMatterList
                                {
                                    ExhibitListId = b.ExhibitListId,
                                    MatterId = a.MatterId,
                                    ExhiId = b.Id,
                                    Title = b.Title,
                                    BriefReference = b.BriefReference,
                                    ListOrder = b.ListOrder
                                });
            }
            else
            {
                exhibitLists = (from a in DataContext.ExhibitLists
                                join b in DataContext.Exhibits on a.Id equals b.ExhibitListId
                                where a.UserId == user.UserID
                                && a.MatterId == matterid
                                select new AllExhibitByMatterList
                                {
                                    ExhibitListId = b.ExhibitListId,
                                    MatterId = a.MatterId,
                                    ExhiId = b.Id,
                                    Title = b.Title,
                                    BriefReference = b.BriefReference,
                                    ListOrder = b.ListOrder
                                });
            }

            return exhibitLists;
        }


        /// <summary>
        /// Gets the exhibit.
        /// </summary>
        /// <param name="exhibitid">The exhibit id.</param>
        /// <returns></returns>
        public Exhibit ExhibitById(int exhibitid)
        {
            return DataContext.Exhibits.Where(b => b.Id == exhibitid).FirstOrDefault();
        }

        /// <summary>
        /// Gets the brief.
        /// </summary>
        /// <param name="briefId">The brief id.</param>
        /// <returns></returns>
        public IQueryable<Mecro> MecroList(int briefId)
        {
            return DataContext.Mecro.Where(b => !b.IsGlobal && b.BriefID.HasValue && b.BriefID == briefId);
        }

        public IQueryable<Sample> SampleList(int briefId)
        {
            return DataContext.Sample.Where(d => !d.IsGlobal && d.BriefID.HasValue && d.BriefID == briefId);
        }

        /// <summary>
        /// Gets the brief.
        /// </summary>
        /// <param name="briefId">The brief id.</param>
        /// <returns></returns>
        public IQueryable<Mecro> GlobalMecroList()
        {
            return DataContext.Mecro.Where(b => b.IsGlobal && !b.BriefID.HasValue);
        }
        public IQueryable<Sample> GlobalSampleList()
        {
            return DataContext.Sample.Where(d=> d.IsGlobal && !d.BriefID.HasValue);
        }

        public void SaveMecro(Mecro mecro)
        {
            if (mecro != null)
            {
                if (mecro.Id > 0)
                {
                    var updateddata = DataContext.Mecro.Where(w => w.Id == mecro.Id).FirstOrDefault();

                    if (updateddata != null)
                    {
                        updateddata.Name = mecro.Name;
                        updateddata.FileName = mecro.FileName;
                        updateddata.UpdatedDate = DateTime.UtcNow;
                        updateddata.UpdatedBy = mecro.UpdatedBy;
                        if (mecro.ExhibitId.HasValue && mecro.ExhibitId.Value > 0)
                        {
                            updateddata.ExhibitId = mecro.ExhibitId;
                        }
                    }
                }
                else
                {
                    DataContext.Mecro.InsertOnSubmit(mecro);
                }

                DataContext.SubmitChanges();
            }
        }

        public void SaveSample(Sample sample)
        {
            if (sample != null)
            {
                if (sample.Id > 0)
                {
                    var updateddata = DataContext.Sample.Where(x => x.Id == sample.Id).FirstOrDefault();

                    if (updateddata != null)
                    {
                        updateddata.Name = sample.Name;
                        updateddata.UpdatedDate = DateTime.UtcNow;
                        updateddata.UpdatedBy = sample.UpdatedBy;
                        //if (mecro.ExhibitId.HasValue && mecro.ExhibitId.Value > 0)
                        //{
                        //    updateddata.ExhibitId = mecro.ExhibitId;
                        //}
                    }
                }
                else
                {
                    DataContext.Sample.InsertOnSubmit(sample);
                }

                DataContext.SubmitChanges();
            }
        }

        /// <summary>
        /// Delete mecro
        /// </summary>
        /// <param name="mecroid"></param>
        public void DeleteMecro(int mecroid)
        {
            if (mecroid > 0)
            {
                var mecrodata = DataContext.Mecro.Where(w => w.Id == mecroid).FirstOrDefault();

                if (mecrodata != null)
                {
                    DataContext.Mecro.DeleteOnSubmit(mecrodata);
                    DataContext.SubmitChanges();
                }
            }

        }

        public void DeleteSample(int sampleid)
        {
            if (sampleid > 0)
            {
                var sampledata = DataContext.Sample.Where(x => x.Id == sampleid).FirstOrDefault();

                if (sampledata != null)
                {
                    DataContext.Sample.DeleteOnSubmit(sampledata);
                    DataContext.SubmitChanges();
                }
            }

        }

        /// <summary>
        /// GetMecro
        /// </summary>
        /// <param name="mecroid"></param>
        /// <returns></returns>
        public Mecro GetMecro(int mecroid)
        {
            return DataContext.Mecro.Where(w => w.Id == mecroid).FirstOrDefault();
        }

        public Sample GetSample(int sampleid)
        {
            return DataContext.Sample.Where(x => x.Id == sampleid).FirstOrDefault();
        }
    }
}

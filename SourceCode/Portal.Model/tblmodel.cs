using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects.DataClasses;
using System.Web.Mvc;
using Portal.Model;

namespace BlueLabs.Models
{
	/// <summary>
	/// This class is used to Define Model for Table - tblUsers
	/// </summary>
	/// <CreatedBy>Disha Mehta</CreatedBy>
	/// <CreatedDate>27-Jan-2014</CreatedDate>
	public class TblUserModel
	{
		#region Properties

		/// <summary>
		/// Gets or sets the UserID value.
		/// </summary>
		[Key]
		public long UserID { get; set; }

		/// <summary>
		/// Gets or sets the DateCreated value.
		/// </summary>
		[Required(ErrorMessage = "*")]
		public DateTime DateCreated { get; set; }

		/// <summary>
		/// Gets or sets the DateModified value.
		/// </summary>
		[Required(ErrorMessage = "*")]
		public DateTime DateModified { get; set; }

		/// <summary>
		/// Gets or sets the FirstName value.
		/// </summary>
		[Required(ErrorMessage = "*")]
		[StringLength(250, ErrorMessage = "*")]
		public string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the LastName value.
		/// </summary>
		[Required(ErrorMessage = "*")]
		[StringLength(250, ErrorMessage = "*")]
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the Email value.
		/// </summary>
		[Required(ErrorMessage = "*")]
		[StringLength(-1, ErrorMessage = "*")]
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the Password value.
		/// </summary>
		[Required(ErrorMessage = "*")]
		[StringLength(-1, ErrorMessage = "*")]
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the Table Name value.
		/// </summary>
        //[TableAttribute(false)]
        //public override string TableName { get { return "tblUsers"; } }

		#endregion
	}

    public class LoginModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    [MetadataType(typeof(tblAdminUsersMetadata))]
    public partial class tblAdminUsers
    {
        internal sealed class tblAdminUsersMetadata
        {
            // Metadata classes are not meant to be instantiated.
            private tblAdminUsersMetadata()
            {
            }

            public long UserID { get; set; }
            public System.DateTime DateCreated { get; set; }
            public System.DateTime DateModified { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string PWHash { get; set; }
        }
    }


    [MetadataType(typeof(tblRatingsCountMetadata))]
    public partial class tblRatingsCount
    {
        internal sealed class tblRatingsCountMetadata
        {
            // Metadata classes are not meant to be instantiated.
            private tblRatingsCountMetadata()
            {
            }

            public long CountID { get; set; }
            [Required]
            public long UserID { get; set; }
            [Required]
            public long LocationID { get; set; }
            [Required]
            public DateTime DateCreated { get; set; }
            [Required]
            public DateTime DateModified { get; set; }
            [Required]
            public long NumberOfRatings { get; set; }

            public tblLocation tblLocation { get; set; }
            public tblUser tblUser { get; set; }
        }
    }


    [MetadataType(typeof(tblRatingMetadata))]
    public partial class tblRating
    {
        internal sealed class tblRatingMetadata
        {
            // Metadata classes are not meant to be instantiated.
            private tblRatingMetadata()
            {
            }
            public long RatingID { get; set; }
            [Required]
            public long UserID { get; set; }
            [Required]
            public long EmployeeID { get; set; }
            [Required]
            public DateTime DateCreated { get; set; }
            [Required]
            public DateTime DateModified { get; set; }
            [Required]
            public string UserName { get; set; }
            [Required]
            public string LocationName { get; set; }
            [Required]
            public string EmployeeName { get; set; }
            [Required]
            public double EmployeeRating { get; set; }
            [Required]
            public long AttitudeRating { get; set; }
            [Required]
            public long SpeedRating { get; set; }
            [Required]
            public long KnowledgeRating { get; set; }
            [Required]
            public bool KiipRewardFlag { get; set; }

            public tblEmployee tblEmployee { get; set; }
            public tblUser tblUser { get; set; }
        }
    }


    [MetadataType(typeof(tblLocationMetadata))]
    public partial class tblLocation
    {
        internal sealed class tblLocationMetadata
        {
            // Metadata classes are not meant to be instantiated.
            private tblLocationMetadata()
            {
            }
            public long LocationID { get; set; }
            [Required]
            public long EstablishmentID { get; set; }
            [Required]
            public DateTime DateCreated { get; set; }
            [Required]
            public DateTime DateModified { get; set; }
            
            public string GooglePlacesID { get; set; }
            [Required]
            public string LocationName { get; set; }
            
            public long? LocationNumber { get; set; }
            
            public string EstablishmentName { get; set; }
            [Required]
            public string StreetAddress { get; set; }
            [Required]
            public string City { get; set; }
            [Required]
            public string State { get; set; }
            [Required]
            public string Country { get; set; }
            
            public string Phone { get; set; }
            
            public decimal? Latitude { get; set; }
            
            public decimal? Longitude { get; set; }
            [Required]
            public long NumberOfRatings { get; set; }
            
            public string Pincode { get; set; }
            
            public double? LocationRating { get; set; }

            public EntityCollection<tblEmployee> tblEmployees { get; set; }
            public tblEstablishment tblEstablishment { get; set; }
            public EntityCollection<tblFavorite> tblFavorites { get; set; }
            public EntityCollection<tblRatingsCount> tblRatingsCounts { get; set; }
        }
    }
        
    [MetadataType(typeof(tblFavoriteMetadata))]
    public partial class tblFavorite
    {
        internal sealed class tblFavoriteMetadata
        {
            // Metadata classes are not meant to be instantiated.
            private tblFavoriteMetadata()
            {
            }
            public long FavoritesID { get; set; }
            [Required]
            public long UserID { get; set; }
            [Required]
            public long LocationID { get; set; }
            [Required]
            public DateTime DateCreated { get; set; }
            [Required]
            public DateTime DateModified { get; set; }
            [Required]
            public string LocationName { get; set; }

            public tblLocation tblLocation { get; set; }
            public tblUser tblUser { get; set; }
        }
    }


    [MetadataType(typeof(tblEstablishmentMetadata))]
    public partial class tblEstablishment
    {
        internal sealed class tblEstablishmentMetadata
        {
            // Metadata classes are not meant to be instantiated.
            private tblEstablishmentMetadata()
            {
            }

            public long EstablishmentID { get; set; }
            [Required]
            public DateTime DateCreated { get; set; }
            [Required]
            public DateTime DateModified { get; set; }
            [Required]
            public string EstablishmentName { get; set; }
            
            public string Logo { get; set; }

            public EntityCollection<tblLocation> tblLocations { get; set; }
        }
    }


    [MetadataType(typeof(tblEmployeeMetadata))]
    public partial class tblEmployee
    {
        internal sealed class tblEmployeeMetadata
        {
            // Metadata classes are not meant to be instantiated.
            private tblEmployeeMetadata()
            {
            }

            public long EmployeeID { get; set; }
            [Required]
            public long LocationID { get; set; }
            [Required]
            public System.DateTime DateCreated { get; set; }
            [Required]
            public System.DateTime DateModified { get; set; }
            
            public string LocationName { get; set; }
            [Required]
            public string FirstName { get; set; }
            
            public string LastName { get; set; }
            
            public long? EmployeeNumber { get; set; }
            
            public string EmployeeTitle { get; set; }
            [Required]
            public long NumberOfRatings { get; set; }

            public double? EmployeeRating { get; set; }

            public tblLocation tblLocation { get; set; }
            public EntityCollection<tblRating> tblRatings { get; set; }
        }
    }

}

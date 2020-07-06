using Api.Models;
using Common;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Test
{
    //MAKE SURE TO HAVE THE AZURE COSMOS DB EMULATOR ON
    public class ControllerTests : BaseTest
    {
        public ControllerTests() : base()
        {

        }


        [Fact]
        public async void GetAllCards()
        {
            var result = await cardController.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var cards = Assert.IsType<List<CardModel>>(okResult.Value);

            Assert.Equal(3, cards.Count());
        }


        [Fact]
        public async void CountCards()
        {
            var result = await cardController.Count();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var count = Assert.IsType<int>(okResult.Value);

            Assert.Equal(3, count);
        }

        [Fact]
        public async void CountAttachedCards()
        {
            var result = await cardController.CountAttachedCards();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var count = Assert.IsType<int>(okResult.Value);

            Assert.Equal(1, count);
        }


        [Fact]
        public async void GetExistingCard()
        {
            var result = await cardController.Get("code1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var card = Assert.IsType<CardModel>(okResult.Value);

            Assert.Equal("card1", card.Name);
            Assert.Equal("code1", card.CardCode);
            Assert.Equal(1, card.Id);

        }


        [Fact]
        public async void PostPutDeleteGetCard()
        {
            //post
            var cardCode = Guid.NewGuid().ToString();

            var model = new CardRegistrationModel
            {
                CardCode = cardCode,
                Name = "card1"
            };

            var result = await cardController.Post(model);

            Assert.IsType<ActionResult<PostCardResult>>(result);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var postCardResult = Assert.IsType<PostCardResult>(createdResult.Value);

            Assert.True(postCardResult.Succeed);

            Assert.Equal(cardCode, postCardResult.CardCode);

            var searchResult = await cardController.Search(new SearchCardFilter { Code = postCardResult.CardCode });

            var searchCardsResult = Assert.IsType<OkObjectResult>(searchResult.Result);

            var cards = Assert.IsType<List<CardModel>>(searchCardsResult.Value);

            var cardModel = cards.FirstOrDefault();

            var card = await genericService.GetAsync<Card>(cardModel.Id);

            Assert.Equal(card.Id, cardModel.Id);
            Assert.Equal(model.Name, card.Name);
            Assert.Equal(cardCode, card.CardCode);
            Assert.Equal(MockedNow, card.Created);
            Assert.NotNull(card.CreatedBy);
            Assert.Equal("javier", card.CreatedBy);
            Assert.Null(card.Updated);
            Assert.Null(card.UpdatedBy);
            Assert.Null(card.IsDisabled);

            //put
            var updateModel = new UpdateCardModel
            {
                Name = "card2",
            };

            var updateResult = await cardController.Put(card.Id, updateModel);
            var ok = Assert.IsType<OkObjectResult>(updateResult.Result);
            var rowsAffected = Assert.IsType<int>(ok.Value);
            Assert.Equal(1, rowsAffected);
            card = await genericService.GetAsync<Card>(card.Id);
            Assert.Equal(updateModel.Name, card.Name);
            Assert.Equal(cardCode, card.CardCode);
            Assert.Equal(MockedNow, card.Updated);
            Assert.NotNull(card.UpdatedBy);
            Assert.Null(card.IsDisabled);

            //delete
            var deleteResult = await cardController.Delete(card.CardCode);
            var deletedOk = Assert.IsType<OkObjectResult>(deleteResult.Result);
            var deletedRowsAffected = Assert.IsType<int>(deletedOk.Value);
            Assert.Equal(1, deletedRowsAffected);

            //get
            var getResult = await cardController.Get(card.CardCode);
            Assert.IsType<NotFoundObjectResult>(getResult.Result);

        }



        [Fact]
        public async void PostGetPutGetDeleteGetSchool()
        {
            var schoolAddress = "street 123";
            var schoolEmail = "school@test.com";
            var schoolLat = 25.7846352;
            var schoolLng = -80.2343637;
            var schoolLogo = "http://google.com";
            var schoolName = "school1";
            var schoolPhone = "123456789";

            var model = new SchoolRegistrationModel
            {
                Address = schoolAddress,
                Email = schoolEmail,
                Lat = schoolLat,
                Lng = schoolLng,
                LogoUrl = schoolLogo,
                Name = schoolName,
                Phone = schoolPhone
            };

            //post
            var postResult = await schoolController.Post(model);
            var createdResult = Assert.IsType<CreatedResult>(postResult.Result);
            var schoolModel = Assert.IsType<SchoolModel>(createdResult.Value);
            Assert.Equal(schoolName, schoolModel.Name);
            Assert.Equal(schoolPhone, schoolModel.Phone);
            Assert.Equal(schoolLogo, schoolModel.LogoUrl);
            Assert.Equal(schoolLng, schoolModel.Lng);
            Assert.Equal(schoolLat, schoolModel.Lat);
            Assert.Equal(schoolEmail, schoolModel.Email);
            Assert.Equal(schoolAddress, schoolModel.Address);
            Assert.Null(schoolModel.Director);


            //put
            var updatedScholAddress = "street 456";
            var updatedSchoolEmail = "updatedSchool@test.com";
            var updatedLat = 25.7746352;
            var updatedLng = -80.2243637;
            var updatedLogoUrl = "http://test.com";
            var updatedName = "updatedSchool";
            var updatedPhone = "987654321";

            var updateModel = new UpdateSchoolModel
            {
                Address = updatedScholAddress,
                Email = updatedSchoolEmail,
                Lat = updatedLat,
                Lng = updatedLng,
                LogoUrl = updatedLogoUrl,
                Name = updatedName,
                Phone = updatedPhone
            };

            var putResult = await schoolController.Put(schoolModel.Id, updateModel);

            var okPut = Assert.IsType<OkObjectResult>(putResult.Result);
            var updatedRows = Assert.IsType<int>(okPut.Value);
            Assert.Equal(1, updatedRows);


            //get updated
            var getResult = await schoolController.Get(schoolModel.Id);
            var getokResult = Assert.IsType<OkObjectResult>(getResult.Result);
            var schoolGetModel = Assert.IsType<SchoolModel>(getokResult.Value);
            Assert.Equal(updatedName, schoolGetModel.Name);
            Assert.Equal(updatedPhone, schoolGetModel.Phone);
            Assert.Equal(updatedLogoUrl, schoolGetModel.LogoUrl);
            Assert.Equal(updatedLng, schoolGetModel.Lng);
            Assert.Equal(updatedLat, schoolGetModel.Lat);
            Assert.Equal(updatedSchoolEmail, schoolGetModel.Email);
            Assert.Equal(updatedScholAddress, schoolGetModel.Address);
            Assert.Null(schoolGetModel.Director);

            //delete
            var deletedSchoolId = schoolGetModel.Id;
            var deleteResult = await schoolController.Delete(schoolGetModel.Id);
            var deletedOk = Assert.IsType<OkObjectResult>(deleteResult.Result);
            var deletedRowsAffected = Assert.IsType<int>(deletedOk.Value);
            Assert.Equal(1, deletedRowsAffected);


            //get deleted
            var result = await schoolController.Get(deletedSchoolId);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(ErrorConstants.SchoolNotFound, notFoundResult.Value);

        }


        [Fact]
        public async void GetNotFoundSchool()
        {
            var notExistingSchoolId = 990;
            var result = await schoolController.Get(notExistingSchoolId);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(ErrorConstants.SchoolNotFound, notFoundResult.Value);
        }


        [Fact]
        public async void PostDirectorGetUpdateSchool()
        {
            var schoolAddress = "street 123";
            var schoolEmail = "school1@test.com";
            var schoolLat = 25.7746352;
            var schoolLng = -80.2243637;
            var schoolLogo = "http://google.com";
            var schoolName = "school1";
            var schoolPhone = "123456789";

            var model = new SchoolRegistrationModel
            {
                Address = schoolAddress,
                Email = schoolEmail,
                Lat = schoolLat,
                Lng = schoolLng,
                LogoUrl = schoolLogo,
                Name = schoolName,
                Phone = schoolPhone
            };

            //post school
            var postResult = await schoolController.Post(model);

            var createdResult = Assert.IsType<CreatedResult>(postResult.Result);
            var schoolModel = Assert.IsType<SchoolModel>(createdResult.Value);


            //post director
            var directorRegistrationModel = new DirectorRegistrationModel();

            var directorEmail = "director@test.com";
            var directorFirstName = "directorFirst";
            var directorLastName = "directorLast";
            var directorPhone = "1321242";


            directorRegistrationModel.Email = directorEmail;
            directorRegistrationModel.FirstName = directorFirstName;
            directorRegistrationModel.LastName = directorLastName;
            directorRegistrationModel.Phone = directorPhone;

            var postDirectorResult = await schoolController.PostDirector(schoolModel.Id, directorRegistrationModel);

            var directorCreatedResult = Assert.IsType<CreatedResult>(postDirectorResult.Result);

            var directorModel = Assert.IsType<UserModel>(directorCreatedResult.Value);

            Assert.Equal(directorRegistrationModel.FirstName, directorModel.FirstName);
            Assert.Equal(directorRegistrationModel.LastName, directorModel.LastName);
            Assert.Equal(directorRegistrationModel.Email, directorModel.Email);
            Assert.Equal(directorRegistrationModel.Phone, directorModel.Phone);

            var directors = await userService.FindAsync<User>(x => x.Id == directorModel.Id);

            var director = directors.First();

            Assert.Equal(UserCategory.SchoolDirector, director.Category);

        }



        [Fact]
        public async void PostStudentComplete()
        {
            //post school
            const string schoolAddress = "street 123";
            const string schoolEmail = "school2@test.com";
            const double schoolLat = 25.7746352;
            const double schoolLng = -80.2243637;
            const string schoolLogo = "http://google.com";
            const string schoolName = "school2";
            const string schoolPhone = "123456789";

            var model = new SchoolRegistrationModel
            {
                Address = schoolAddress,
                Email = schoolEmail,
                Lat = schoolLat,
                Lng = schoolLng,
                LogoUrl = schoolLogo,
                Name = schoolName,
                Phone = schoolPhone
            };


            var postResult = await schoolController.Post(model);
            var createdResult = Assert.IsType<CreatedResult>(postResult.Result);
            Assert.IsType<SchoolModel>(createdResult.Value);

            //post card
            var cardRegistrationModel = new CardRegistrationModel
            {
                CardCode = Guid.NewGuid().ToString(),
                Name = "card1"
            };

            await cardController.Post(cardRegistrationModel);

            var cardRegistrationModel2 = new CardRegistrationModel
            {
                CardCode = Guid.NewGuid().ToString(),
                Name = "card2"
            };


            var result = await cardController.Post(cardRegistrationModel2);

            Assert.IsType<ActionResult<PostCardResult>>(result);

            var cardCreatedResult = Assert.IsType<CreatedResult>(result.Result);
            var postCardResult2 = Assert.IsType<PostCardResult>(cardCreatedResult.Value);


            var searchResult = await cardController.Search(new SearchCardFilter { Code = postCardResult2.CardCode });

            var searchOkResult = Assert.IsType<OkObjectResult>(searchResult.Result);

            var cards = Assert.IsType<List<CardModel>>(searchOkResult.Value);

            var cardModel2 = cards.FirstOrDefault();

            //post parent
            var parentRegistrationModel = new ParentRegistrationModel
            {
                Email = "parent@test.com",
                FirstName = "Javier",
                LastName = "Acrich",
                Password = "P@assword1234",
                Phone = "123456789"
            };


            var postParentResult = await userController.PostParent(parentRegistrationModel);

            var createdParentResult = Assert.IsType<CreatedResult>(postParentResult.Result);

            var parentModel = Assert.IsType<UserModel>(createdParentResult.Value);

            Assert.Equal(parentRegistrationModel.Email, parentModel.Email);
            Assert.Equal(parentRegistrationModel.FirstName, parentModel.FirstName);
            Assert.Equal(parentRegistrationModel.LastName, parentModel.LastName);
            Assert.Equal(parentRegistrationModel.Phone, parentModel.Phone);

            var parents = await userService.FindAsync<User>(x => x.Id == parentModel.Id);

            var parent = parents.First();

            Assert.Equal(UserCategory.Parent, parent.Category);


            //post student

            var studentRegistrationModel = new StudentRegistrationModel
            {
                FirstName = "Dante",
                LastName = "Acrich",
                Phone = "654654654",
                SchoolId = 1,
                StudentId = "abc",
                Grade = SchoolGrade.Elementary,
                CardCode = cardRegistrationModel.CardCode
            };

            var postStudentResult = await userController.PostStudent(Guid.Parse(parent.Id), studentRegistrationModel);

            //validate post student result;

            var createdStudentResult = Assert.IsType<CreatedResult>(postStudentResult.Result);

            var studentModel = Assert.IsType<UserModel>(createdStudentResult.Value);

            Assert.Equal(studentRegistrationModel.FirstName, studentModel.FirstName);
            Assert.Equal(studentRegistrationModel.LastName, studentModel.LastName);

            Assert.Equal(studentRegistrationModel.Grade, studentModel.Grade);
            Assert.Equal(studentRegistrationModel.Phone, studentModel.Phone);
            Assert.Equal(studentRegistrationModel.SchoolId, studentModel.School.Id);
            Assert.Equal(studentRegistrationModel.StudentId, studentModel.StudentId);
            Assert.Equal(studentRegistrationModel.CardCode, studentModel.Cards.First().CardCode);

            var cards1 = await genericService.FindAsync<Card>(x => x.CardCode == studentRegistrationModel.CardCode);

            var card = cards1.FirstOrDefault();
            Assert.Equal(card.Id, studentModel.Cards.First().Id);


            //check card history item created

            var historyItemList = await genericService.FindAsync<StudentCardHistoryItem>(x => x.UserId == studentModel.Id);

            var historyItem = historyItemList.FirstOrDefault();

            Assert.NotNull(historyItem);

            Assert.Single(historyItemList);

            //check parent has a student
            Assert.Single(parent.Students);

            //update student
            studentModel.FirstName = "updatedName";

            var updateUserModel = new UpdateUserModel
            {
                FirstName = "updatedFirstName",
                LastName = "updatedLastName",
                Email = "updated@test.com",
                Grade = SchoolGrade.High,
                Phone = "987987978",
                PhotoUrl = "https://photourl.com",
                Card = cardModel2 //creates a histoy item
            };

            var studentPutResult = await userController.Put(Guid.Parse(studentModel.Id), updateUserModel);

            var updatedStudentResult = Assert.IsType<OkObjectResult>(studentPutResult.Result);

            var identityResult = Assert.IsType<IdentityResult>(updatedStudentResult.Value);

            Assert.True(identityResult.Succeeded);

            var users = await userService.FindAsync<User>(x => x.FirstName == updateUserModel.FirstName);

            var user = users.FirstOrDefault();

            Assert.Equal(updateUserModel.FirstName, user.FirstName);
            Assert.Equal(updateUserModel.LastName, user.LastName);
            Assert.Equal(updateUserModel.Email, user.Email);
            Assert.Equal(updateUserModel.Grade, user.Grade);
            Assert.Equal(updateUserModel.Phone, user.PhoneNumber);
            Assert.Equal(updateUserModel.PhotoUrl, user.PhotoUrl);

            var historyItems = await genericService.FindAsync<StudentCardHistoryItem>(x => x.UserId == studentModel.Id);

            Assert.Equal(2, historyItems.Count());

            //update card name
            var updateCardModel = new UpdateCardModel
            {
                Name = "updated card name"
            };

            await cardController.Put(card.Id, updateCardModel);

            var cardList = await genericService.FindAsync<Card>(x => x.Name == updateCardModel.Name);

            Assert.Single(cardList);

            //cleanup
            await cardController.Delete(card.CardCode);
            await cardController.Delete(cardRegistrationModel2.CardCode);

            //todo: get student location


        }





        [Fact]
        public async void GetAllDevices()
        {
            var result = await deviceController.GetAll();
            var getResult = Assert.IsType<OkObjectResult>(result.Result);
            var devices = Assert.IsType<List<DeviceModel>>(getResult.Value);
            Assert.Equal(4, devices.Count);
        }

        [Fact]
        public async void GetSingleDevice()
        {
            var result = await deviceController.Get("aaaa#1");
            var getResult = Assert.IsType<OkObjectResult>(result.Result);
            var device = Assert.IsType<DeviceModel>(getResult.Value);
            Assert.Equal("AAAA#1", device.DeviceCode);
        }

        [Fact]
        public async void RelateDevices()
        {
            var model = new RelateDevicesModel()
            {
                InsideDeviceCode = "AAAA#2",
                OutsideDeviceCode = "aaaa#1",
            };

            var relateResult = await deviceController.PatchRelate(model);

            var rowsAffected = Assert.IsType<OkObjectResult>(relateResult.Result);
            var devices = Assert.IsType<int>(rowsAffected.Value);
            Assert.Equal(2, devices);

        }

        [Fact]
        public async void GetSingleNotFoundDevice()
        {
            var result = await deviceController.Get("abcd#1");
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(ErrorConstants.DeviceNotFound, notFoundResult.Value);
        }

        [Fact]
        public async void PostDeviceAttachedToSchool()
        {

            var device3 = new DeviceRegistrationModel()
            {
                Name = "device3",
                DeviceCode = "AAAA#3",
                SchoolId = 1,
                Type = DeviceType.Outside,
            };

            var result = await deviceController.Post(device3);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);


            var deleteResult = await deviceController.Delete(device3.DeviceCode);

            var okResult = Assert.IsType<OkObjectResult>(deleteResult.Result);
            var rowsAffected = Assert.IsType<int>(okResult.Value);
            Assert.Equal(1, rowsAffected);

        }

        [Fact]
        public async void PostDeviceAttachedToBus()
        {


            var device4 = new DeviceRegistrationModel()
            {
                Name = "device4",
                DeviceCode = "CCCC#4",
                SchoolBusId = 1,
            };

            var result = await deviceController.Post(device4);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);

            var deleteResult = await deviceController.Delete(device4.DeviceCode);

            var okResult = Assert.IsType<OkObjectResult>(deleteResult.Result);
            var rowsAffected = Assert.IsType<int>(okResult.Value);
            Assert.Equal(2, rowsAffected);

        }

        [Fact]
        public async void PostDeviceAttachedToBusNotFound()
        {

            var device6 = new DeviceRegistrationModel()
            {
                Name = "device6",
                DeviceCode = "AAAA#6",
                SchoolBusId = 99,
                Type = DeviceType.Outside,
            };

            var result = await deviceController.Post(device6);

            var busNotFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);

            Assert.Equal(ErrorConstants.SchoolBusNotFound, busNotFoundResult.Value);

        }

        [Fact]
        public async void PostDeviceAttachedToSchoolNotFound()
        {

            var device7 = new DeviceRegistrationModel()
            {
                Name = "device7",
                DeviceCode = "AAAA#7",
                SchoolId = 99,
                Type = DeviceType.Outside,
            };

            var result = await deviceController.Post(device7);

            var schoolNotFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);

            Assert.Equal(ErrorConstants.SchoolNotFound, schoolNotFoundResult.Value);

        }


        [Fact]
        public async void PostDeviceDoubleAssociation()
        {

            var device5 = new DeviceRegistrationModel()
            {
                Name = "device5",
                DeviceCode = "AAAA#5",
                SchoolBusId = 1,
                SchoolId = 1,
                Type = DeviceType.Outside,
            };

            var result = await deviceController.Post(device5);

            var createdResult = Assert.IsType<BadRequestObjectResult>(result.Result);

        }

        //[Fact]
        //public async void GetLocation()
        //{

        //    SetCurrentPrincipal();

        //    var location = await userController.GetLocation(this.student1Id);
        //}


    }
}

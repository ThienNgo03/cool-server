namespace Journal.Events
{
    public class Controller
    {
    }
}

// yêu cầu người dùng chọn Exercise và sau đó tự nhập vào số rep, time, set 
//TH1:
//Exercise(Id, name, description, CreatedDate, LastUpdate)
//WorkoutLog(Id, ExId, Rep?, Time?, set, CreatedDate, LastUpdate)

//-------
//TH2:
//Exercise(Id, name, description, CreatedDate, LastUpdated)

//WorkoutLog(Id, WorkoutId, Rep?, Time?, Set, ExDate, CreatedDate, LastUpdated) //thực tế

//Workout(Id, ExId, UserId, CreatedDate, LastUpdated) //Dự định

//WeekPLan(Id, WorkoutId, DateOfWeek, Time, Rep?, HoldingTime?, Set) //Tập vào thứ mấy và mấy giờ



//-------

//Competitions(Id, ExerciseId, Title, Location, DateTime, Type, MaxPlayers) 
//-Id1-ExerciseId1-ChayDua-S10.02-17h-Ranked
//-Id2-ExerciseId2-VatTay-S10.02-15h-Solo


//SoloPool(Id, CompetionId, WinnerId, LoserId)
//-Id1-CompetionId2-Viet-Khoa


//Game(name, maximum player)

//WinnerPool(Id, CompetionId, UserId, Position)
//-Id1-CompetionId1-Thien-1
//-Id2-CompetionId1-Khoa-2
//-Id3-CompetionId1-Toan-3
//-Id4-CompetionId1-Viet-4
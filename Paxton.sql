use Net2

--select top 10 AccessLevelId, *
select *
from users
where surname = 'Burnett' 
and firstname = 'Dan'

--where /*firstname = 'Annabel Turner / Benedict Heaney'
--and*/ (Field14_50 is null or Field14_50 = '') -- Field14_50 shows in the UI as personnel number
--and userid > 0 -- system users are 0 or less
--and active = 1 -- I think "deleting" a user in the gui sets this to 0




--begin tran

--select top 100 *
--from users

--update users set departmentid = 0
--where departmentid <> 0

--select top 100 *
--from users

--commit

--select top 100 * from departments

--select top 100 * from [Access levels]

--sp_tables

--select top 10 AccessLevelId, *
--from users
--where firstname = 'Temp user' --'Payne' --and firstname = 'Robert'

--select * -- Field14_50 as BEC_Number
--from users
--where Field14_50 is not null and Field14_50 <> ''

--select *
--from users
--where firstname + ' ' + surname in 
--(
--	select firstname + ' ' + surname --, count(firstname + ' ' + surname)
--	from users
--	group by firstname + ' ' + surname
--	having count(firstname + ' ' + surname) > 1
--)
--order by firstname + ' ' + surname

--select top 1000 *
--from users
--where field13_Memo is not null and Field13_Memo not like ''


use Net2

--select top 10 *
--from users
--where surname = 'Adams' and firstname = 'Robert'

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

select top 1000 *
from users
where field13_Memo is not null
### UK general election results 2019 webscraper
A C# console app that "webscrapes" the results of the 2019 general election from the BBC website and saves the data in csv format. By changing a preprocessor directive the app can be modified to save the data to a SQL Server database instead of or as well as the csv files. One of the data sets can also be downloaded as a JSON file because it appears in that form on the BBC site. This is also controlled by a preprocessor directive. Download location and/or database connection can be set by changing the value of the following variables:
* DataAccess.connectionString
* csvDir

Requirements:
* HtmlAgilityPack
* CsvHelper
* Newtonsoft.Json

#### CSV files
##### Constituency

<table>
<thead>
	<tr>
		<th>Column</th>
		<th>Description</th>
	</tr>
</thead>
<tbody>
	<tr>
		<td>conID</td>
		<td>Database generated primary key</td>
	</tr>
	<tr>
		<td>conName</td>
		<td>Name of constituency</td>
	</tr>
	<tr>
		<td>ons</td>
		<td>Ordnance survey identifier</td>
	</tr>
</tbody>
</table>

##### ConstituencyResult

<table>
<thead>
	<tr>
		<th>Column</th>
		<th>Description</th>
	</tr>
</thead>
<tbody>
	<tr>
		<td>conID</td>
		<td>Database generated constituency identifier</td>
	</tr>
	<tr>
		<td>electionID</td>
		<td>Database generated constituency identifier</td>
	</tr>
	<tr>
		<td>winningParty</td>
		<td>Winning party short identifier</td>
	</tr>
	<tr>
		<td>headline</td>
		<td>Result as in "GAIN FROM LAB", "HOLD" etc</td>
	</tr>
	<tr>
		<td>majority</td>
		<td>Winning candidate's margin of victory over second place</td>
	</tr>
	<tr>
		<td>electorate</td>
		<td>Registered voters in constituency</td>
	</tr>
	<tr>
		<td>turnout</td>
		<td>Percentage of electorate who voted</td>
	</tr>
	<tr>
		<td>turnoutChange</td>
		<td>Change in percentage of electorate who voted</td>
	</tr>
</tbody>
</table>

##### CandidateResult

<table>
<thead>
	<tr>
		<th>Column</th>
		<th>Description</th>
	</tr>
</thead>
<tbody>
	<tr>
		<td>conID</td>
		<td>Database generated constituency identifier</td>
	</tr>
	<tr>
		<td>electionID</td>
		<td>Database generated constituency identifier</td>
	</tr>
	<tr>
		<td>partyCode</td>
		<td>Short party identifier</td>
	</tr>
	<tr>
		<td>partyName</td>
		<td>Full party name</td>
	</tr>
	<tr>
		<td>candidateName</td>
		<td>Candidate name</td>
	</tr>
	<tr>
		<td>votes</td>
		<td>Votes for candidate</td>
	</tr>
	<tr>
		<td>voteShare</td>
		<td>Percentage share of vote</td>
	</tr>
	<tr>
		<td>voteShareChange</td>
		<td>Change in percentage share of vote</td>
	</tr>
</tbody>
</table>

##### ConstituencyHead

<table>
<thead>
	<tr>
		<th>Column</th>
		<th>Description</th>
	</tr>
</thead>
<tbody>
	<tr>
		<td>ons</td>
		<td>Ordnance survey constituency identifier</td>
	</tr>
	<tr>
		<td>wp</td>
		<td>Winning party short identifier</td>
	</tr>
	<tr>
		<td>wpp</td>
		<td>Winning party at previous election short identifier</td>
	</tr>
	<tr>
		<td>flash</td>
		<td>Result as in "GAIN FROM LAB", "HOLD" etc</td>
	</tr>
</tbody>
</table>

See GER.sql for SQL Server database tables and stored procedures. 


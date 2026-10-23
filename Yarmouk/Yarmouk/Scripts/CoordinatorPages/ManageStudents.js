// Form fields for accounts loaded by ASP.NET.
(() => {
    const departments = ['Computer Science','Software Engineering','Cybersecurity','Computer Information Systems','Computer Information Technology','Data Science and Artificial Intelligence'].map(value => ({value,label:value}));
    const supervisors = [{value:'',label:'Unassigned'},];
    window.YarmoukRoster.mount({
        page:'ManageStudents', title:'Students', singular:'student', plural:'students',
        columns:[{key:'name',type:'identity'},{key:'department'},{key:'gpa',type:'gpa'},{key:'college'},{key:'supervisor',type:'supervisor',options:supervisors}],
        fields:[
            {key:'name',label:'Name (Arabic)',required:true}, {key:'email',label:'Email address',type:'email',required:true},
            {key:'department',label:'Department',options:departments,required:true}, {key:'gpa',label:'GPA (0–100)',type:'number',min:0,max:100,step:0.01,required:true},
            {key:'college',label:'College',default:'Faculty of IT and Computer Science',required:true}, {key:'supervisor',label:'Assigned supervisor',options:supervisors},
            {key:'englishName',label:'Name (English)'}, {key:'studentNumber',label:'Student number',inputMode:'numeric',pattern:'[0-9]+',description:'Digits only; leading zeros are kept.'},
            {key:'nationalId',label:'National ID',inputMode:'numeric',pattern:'[0-9]+',description:'Digits only; leading zeros are kept.'}, {key:'gender',label:'Gender',options:[{value:'',label:'Not specified'},{value:'male',label:'Male'},{value:'female',label:'Female'}]},
            {key:'nationality',label:'Nationality'}, {key:'birthPlace',label:'Place of birth'},
            {key:'birthDate',label:'Date of birth',type:'date',max:new Date().toLocaleDateString('en-CA')}, {key:'academicStatus',label:'Academic status'},
            {key:'completedCredits',label:'Completed credits',type:'number',min:0,step:1,default:0,required:true}, {key:'expectedGraduation',label:'Expected graduation'}
        ],
        records:[]
    });
})();

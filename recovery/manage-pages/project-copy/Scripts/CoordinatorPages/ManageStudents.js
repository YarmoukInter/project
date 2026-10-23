// Form fields for accounts loaded by ASP.NET.
(() => {
    const departments = ['Computer Science','Software Engineering','Cybersecurity','Computer Information Systems','Computer Information Technology','Data Science and Artificial Intelligence'].map(value => ({value,label:value}));
    const supervisors = [{value:'',label:'Unassigned'},];
    window.YarmoukRoster.mount({
        page:'ManageStudents', title:'Students', singular:'student', plural:'students',
        columns:[{key:'name',type:'identity'},{key:'department'},{key:'gpa',type:'gpa'},{key:'college'},{key:'supervisor',type:'supervisor',options:supervisors}],
        fields:[
            {key:'name',label:'Full name',required:true}, {key:'email',label:'Email address',type:'email',required:true},
            {key:'department',label:'Department',options:departments,required:true}, {key:'gpa',label:'GPA',type:'number',min:0,max:4,step:0.01,required:true},
            {key:'college',label:'College',default:'Faculty of IT and Computer Science',required:true}, {key:'supervisor',label:'Assigned supervisor',options:supervisors}
        ],
        records:[]
    });
})();

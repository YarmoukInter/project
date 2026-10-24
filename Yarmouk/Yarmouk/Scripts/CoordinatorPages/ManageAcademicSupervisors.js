// Form fields for accounts loaded by ASP.NET.
(() => {
    const departments = ['Computer Science','Software Engineering','Cybersecurity','Computer Information Systems','Computer Information Technology','Data Science and Artificial Intelligence'].map(value => ({value,label:value}));
    window.YarmoukRoster.mount({
        page:'ManageAcademicSupervisors',title:'Academic supervisors',singular:'supervisor',plural:'supervisors',
        columns:[{key:'name',label:'Supervisor',type:'identity'},{key:'department',label:'Department'},{key:'college',label:'College'},{key:'assignedStudents',label:'Assigned students',type:'count'},{key:'status',label:'Account status',type:'status'}],
        fields:[
            {key:'name',label:'Full name',required:true},{key:'email',label:'Email address',type:'email',required:true},
            {key:'department',label:'Department',options:departments,required:true},{key:'college',label:'College',default:'Faculty of IT and Computer Science',required:true},
            {key:'status',label:'Account status',options:[{value:'Active',label:'Active'},{value:'Inactive',label:'Inactive'}],required:true}
        ],
        records:[]
    });
})();

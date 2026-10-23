// Form fields for accounts loaded by ASP.NET.
window.YarmoukRoster.mount({
    page:'ManageCompanies',title:'Companies',singular:'company',plural:'companies',
    columns:[{key:'name',type:'identity',detailKey:'companyEmail'},{key:'contactName'},{key:'email'},{key:'status',type:'status'}],
    fields:[
        {key:'name',label:'Company name',required:true},{key:'companyEmail',label:'Company email',type:'email'},
        {key:'contactName',label:'HR name',required:true},{key:'email',label:'HR email',type:'email',required:true},
        {key:'industry',label:'Industry',required:true},{key:'location',label:'Location',required:true},
        {key:'status',label:'Approval status',options:[{value:'Approved',label:'Approved'},{value:'Pending review',label:'Pending review'}],required:true}
    ],
    records:[]
});

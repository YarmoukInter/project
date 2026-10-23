// Form fields for accounts loaded by ASP.NET.
window.YarmoukRoster.mount({
    page:'ManageCompanies',title:'Companies',singular:'company',plural:'companies',
    columns:[{key:'name',type:'identity'},{key:'industry'},{key:'location'},{key:'contactName'},{key:'status',type:'status'}],
    fields:[
        {key:'name',label:'Company name',required:true},{key:'email',label:'Contact email',type:'email',required:true},
        {key:'industry',label:'Industry',required:true},{key:'location',label:'Location',required:true},
        {key:'contactName',label:'Contact person',required:true},
        {key:'status',label:'Approval status',options:[{value:'Approved',label:'Approved'},{value:'Pending review',label:'Pending review'}],required:true}
    ],
    records:[]
});

import { AlertifyjsService } from './../_services/alertifyjs.service';
import { ActivatedRoute } from '@angular/router';
import { UserService } from './../_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';
import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination, PaginatedResult } from '../_models/pagination';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  messageContainer = 'Unread';

  constructor(private authService: AuthService, private userService: UserService,
              private route: ActivatedRoute, private alertify: AlertifyjsService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.messages = data['messages'].result;
      this.pagination = data['messages'].pagination;
    })
  }

  loadMessages() {
    this.userService.getMessages(this.authService.decodedToken.nameid 
      ,this.pagination.currentPage, this.pagination.itemsPerPage, this.messageContainer)
      .subscribe((res: PaginatedResult<Message[]>) => {
        this.messages = res.result;
        this.pagination = res.pagination;
      }, error => {
        this.alertify.error(error);
      });
  }

  deleteMessage(id: number) {
    this.alertify.confirm('Are you sure you want to delete this message?' , () => {
      this.userService.deleteMessage(id, this.authService.decodedToken.nameid).subscribe(()=>{
        // the deleteMessage method doesn't return a thing
        // however, just after deleted from DB, we need to
        // delete it from the array that is shown in the html
        this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
        this.alertify.success('Message deleted successfully');
      }, error => {
        this.alertify.error('Failed to delete the message');
      });
    });
  }

  pageChange(event: any) : void{
    this.pagination.currentPage = event.page;
    this.loadMessages();
  }

}
